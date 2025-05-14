using System;

namespace PaymentProcessor.BuildingBlocks.Common.Exceptions
{
    public class BusinessException : Exception
    {
        public string Code { get; }

        public BusinessException(string message) : base(message)
        {
            Code = "BusinessError";
        }

        public BusinessException(string code, string message) : base(message)
        {
            Code = code;
        }

        public BusinessException(string message, Exception innerException) : base(message, innerException)
        {
            Code = "BusinessError";
        }

        public BusinessException(string code, string message, Exception innerException) : base(message, innerException)
        {
            Code = code;
        }
    }
}