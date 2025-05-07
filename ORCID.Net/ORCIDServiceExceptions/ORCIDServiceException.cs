namespace ORCID.Net.ORCIDServiceExceptions;

public class ORCIDServiceException : Exception
{
    public ORCIDServiceException(string exceptionMessage)
        : base(exceptionMessage)
    {
    }
    
    public ORCIDServiceException(string exceptionMessage, Exception innerException)
        : base(exceptionMessage, innerException)
    {
    }
}