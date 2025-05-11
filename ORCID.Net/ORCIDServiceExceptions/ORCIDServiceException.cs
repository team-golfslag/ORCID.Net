namespace ORCID.Net.ORCIDServiceExceptions;

public class OrcidServiceException : Exception
{
    public OrcidServiceException(string exceptionMessage)
        : base(exceptionMessage)
    {
    }
    
    public OrcidServiceException(string exceptionMessage, Exception innerException)
        : base(exceptionMessage, innerException)
    {
    }
}