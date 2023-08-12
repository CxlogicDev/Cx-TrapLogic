using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxUtility.EFCoreData
{

    [Serializable]
    public class DBContextContinueException : Exception
    {
        //public DBContextException() { }
        public DBContextContinueException(string message) : base(message) { }
        public DBContextContinueException(string message, Exception inner) : base(message, inner) { }
        protected DBContextContinueException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

    }



    [Serializable]
    public class DBContextStopException : Exception
    {
        public DBContextStopException() { }
        public DBContextStopException(string message) : base(message) { }
        public DBContextStopException(string message, Exception inner) : base(message, inner) { }
        protected DBContextStopException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class DBContextMultipleEntityException : Exception
    {
        public DBContextMultipleEntityException() { }
        public DBContextMultipleEntityException(string message) : base(message) { }

        public DBContextMultipleEntityException(string message, Exception inner) : base(message, inner) { }

        protected DBContextMultipleEntityException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }



}
