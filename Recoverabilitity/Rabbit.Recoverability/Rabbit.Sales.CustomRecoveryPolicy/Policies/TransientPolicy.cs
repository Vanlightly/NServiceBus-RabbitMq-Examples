using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Rabbit.Sales.CustomRecoveryPolicy.Policies
{
    public interface ITransientDefaultPolicy
    {
        ITransientPolicy PersistentByDefault();
        IPersistentPolicy TransientByDefault();
    }

    public interface ITransientPolicy
    {
        ITransientPolicy ClassifyAsTransient<TException>() where TException : Exception;
        ITransientPolicy ClassifyAsSemiTransient<TException>() where TException : Exception;
        ITransientPolicy AddTransientClassifier(IErrorClassifier errorClassifier);
    }

    public interface IPersistentPolicy
    {
        IPersistentPolicy ClassifyAsPersistent<TException>() where TException : Exception;
        IPersistentPolicy AddPersistentClassifier(IErrorClassifier errorClassifier);
    }
    
    public enum ErrorCategory
    {
        Unknown,
        Persistent,
        SemiTransient,
        Transient
    }

    public interface IErrorClassifier
    {
        ErrorCategory GetCategory(Exception ex);
    }

    public class PolicyInstance : ITransientDefaultPolicy, ITransientPolicy, IPersistentPolicy
    {
        public bool MultipleKeys { get; set; }
        public string PolicyKey { get; private set; }
        public List<string> PolicyKeys { get; private set; }

        private List<IErrorClassifier> _detectors;
        private ErrorCategory _defaultCategory;

        public PolicyInstance(string policyKey)
        {
            PolicyKey = policyKey;

            _detectors = new List<IErrorClassifier>();
        }

        public PolicyInstance(List<string> policyKeys)
        {
            MultipleKeys = true;
            PolicyKeys = policyKeys;

            _detectors = new List<IErrorClassifier>();
        }

        public ITransientPolicy PersistentByDefault()
        {
            _defaultCategory = ErrorCategory.Persistent;

            return this;
        }

        public IPersistentPolicy TransientByDefault()
        {
            _defaultCategory = ErrorCategory.Transient;

            return this;
        }


        public ITransientPolicy ClassifyAsTransient<TException>() where TException : Exception
        {
            var detector = new DefaultErrorClassifier(typeof(TException), ErrorCategory.Transient);
            _detectors.Add(detector);

            return this;
        }

        public ITransientPolicy ClassifyAsSemiTransient<TException>() where TException : Exception
        {
            var detector = new DefaultErrorClassifier(typeof(TException), ErrorCategory.SemiTransient);
            _detectors.Add(detector);

            return this;
        }

        public IPersistentPolicy ClassifyAsPersistent<TException>() where TException : Exception
        {
            var detector = new DefaultErrorClassifier(typeof(TException), ErrorCategory.Persistent);
            _detectors.Add(detector);

            return this;
        }

        public ITransientPolicy AddTransientClassifier(IErrorClassifier errorClassifier)
        {
            _detectors.Add(errorClassifier);

            return this;
        }

        public IPersistentPolicy AddPersistentClassifier(IErrorClassifier errorClassifier)
        {
            _detectors.Add(errorClassifier);

            return this;
        }

        public ErrorCategory GetCategory(Exception ex)
        {
            var category = ErrorCategory.Unknown;

            foreach (var detector in _detectors)
            {
                var thisCategory = detector.GetCategory(ex);
                if (thisCategory == ErrorCategory.Persistent && (category != ErrorCategory.Transient && category != ErrorCategory.SemiTransient))
                    category = ErrorCategory.Persistent;
                else if (thisCategory == ErrorCategory.SemiTransient && category != ErrorCategory.Transient)
                    category = ErrorCategory.SemiTransient;
                else if (thisCategory == ErrorCategory.Transient)
                    category = ErrorCategory.Transient;
            }

            if (category == ErrorCategory.Unknown)
                return _defaultCategory;
            else
                return category;
        }
    }

    public class DefaultErrorClassifier : IErrorClassifier
    {
        private Type _targetType;
        private ErrorCategory _defaultCategory;

        public DefaultErrorClassifier(Type targetType, ErrorCategory defaultCategory)
        {
            _targetType = targetType;
            _defaultCategory = defaultCategory;
        }

        public ErrorCategory GetCategory(Exception ex)
        {
            var exceptionType = ex.GetType();

            if(exceptionType.Equals(_targetType)
                || exceptionType.IsSubclassOf(_targetType))
            {
                return _defaultCategory;
            }

            return ErrorCategory.Unknown;
        }
    }


    public class SqlErrorClassifier : IErrorClassifier
    {
        public ErrorCategory GetCategory(Exception ex)
        {
            if (ex.GetType().Equals(typeof(SqlException)))
            {
                SqlException sqlEx = (SqlException)ex;

                if (sqlEx.Number == 1205 // 1205 = Deadlock
                    || sqlEx.Number == -2 // -2 = TimeOut
                    || sqlEx.Number == -1 // -1 = Connection
                    || sqlEx.Number == 2 // 2 = Connection
                    || sqlEx.Number == 53 // 53 = Connection
                    )
                {
                    return ErrorCategory.Transient;
                }
                else
                {
                    return ErrorCategory.Persistent;
                }
            }

            // if it isn't an SqlException then this detector cannot give an opinion
            return ErrorCategory.Unknown;
        }
    }

    public class FtpErrorClassifier : IErrorClassifier
    {
        public ErrorCategory GetCategory(Exception ex)
        {
            if (ex.GetType().Equals(typeof(WebException)))
            {
                var webException = (WebException)ex;

                var ftpResponse = ((FtpWebResponse)webException.Response);
                if (ftpResponse.StatusCode == FtpStatusCode.ConnectionClosed)
                {
                    return ErrorCategory.Transient;
                }
                else if (ftpResponse.StatusCode == FtpStatusCode.ServiceNotAvailable
                    || ftpResponse.StatusCode == FtpStatusCode.ServiceTemporarilyNotAvailable)
                {
                    return ErrorCategory.SemiTransient;
                }
                else
                {
                    return ErrorCategory.Persistent;
                }
            }

            // if it isn't a WebException then this detector cannot give an opinion
            return ErrorCategory.Unknown;
        }
    }

    internal class PolicyStore
    {
        private static PolicyStore _instance;
        private static object _obj = new object();

        private PolicyStore()
        {
            _policies = new ConcurrentDictionary<string, PolicyInstance>();
        }

        internal static PolicyStore Instance
        {
            get
            {
                lock (_obj)
                {
                    if (_instance == null)
                        _instance = new PolicyStore();

                    return _instance;
                }
            }
        }

        private ConcurrentDictionary<string, PolicyInstance> _policies;

        internal PolicyInstance GetTransientPolicy(string policyKey)
        {
            if (_policies.ContainsKey(policyKey))
                return _policies[policyKey];
            else
                return new PolicyInstance("default");
        }

        internal void AddPolicy(PolicyInstance policyInstance)
        {
            if (policyInstance.MultipleKeys)
            {
                foreach (var policyKey in policyInstance.PolicyKeys)
                    TryAddPolicy(policyKey, policyInstance);
            }
            else
            {
                TryAddPolicy(policyInstance.PolicyKey, policyInstance);
            }
        }

        private void TryAddPolicy(string policyKey, PolicyInstance policyInstance)
        {
            if (!_policies.ContainsKey(policyKey))
            {
                var added = _policies.TryAdd(policyKey, policyInstance);

                if (!added)
                    throw new Exception("A policy already exists for policy key: " + policyKey);
            }
        }
    }


    public static class FailPolicy
    {
        public static ITransientDefaultPolicy CreatePolicyWithName(string policyName)
        {
            var policy = new PolicyInstance(policyName);
            PolicyStore.Instance.AddPolicy(policy);

            return policy;
        }

        public static PolicyInstance GetPolicy(string policyName)
        {
            return PolicyStore.Instance.GetTransientPolicy(policyName);
        }
    }


}
