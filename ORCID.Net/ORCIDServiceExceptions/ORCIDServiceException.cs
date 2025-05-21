// This program has been developed by students from the bachelor Computer Science at Utrecht
// University within the Software Project course.
// 
// © Copyright Utrecht University (Department of Information and Computing Sciences)

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
