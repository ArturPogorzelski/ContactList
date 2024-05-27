using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Infrastructure
{
    [Serializable]
    public class DataException : Exception
    {
        public int? SqlErrorCode { get; private set; }
        public string SqlErrorState { get; private set; }
        public Exception OriginalException { get; }

        public DataException()
        {
        }

        public DataException(string message) : base(message)
        {
        }

        public DataException(string message, Exception innerException)
            : base(message, innerException)
        {
            OriginalException = innerException;

            if (innerException is SqlException sqlEx)
            {
                SqlErrorCode = sqlEx.Number;
                SqlErrorState = sqlEx.State.ToString();
            }
        }

        protected DataException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            OriginalException = (Exception)info.GetValue("OriginalException", typeof(Exception));
            SqlErrorCode = (int?)info.GetValue("SqlErrorCode", typeof(int?));
            SqlErrorState = info.GetString("SqlErrorState");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("OriginalException", OriginalException);
            info.AddValue("SqlErrorCode", SqlErrorCode);
            info.AddValue("SqlErrorState", SqlErrorState);
        }
    }
}
