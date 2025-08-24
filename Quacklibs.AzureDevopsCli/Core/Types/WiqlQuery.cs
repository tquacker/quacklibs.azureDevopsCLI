namespace Quacklibs.AzureDevopsCli.Core.Types
{
    public class WiqlQuery
    {
    }

    public class WiqlQueryPart
    {
    }

    public class AssignedUserWiqlQueryPart : WiqlQueryPart
    {
        private readonly string _defaultUserEmail;

        public AssignedUserWiqlQueryPart(string defaultUserEmail)
        {
            _defaultUserEmail = defaultUserEmail;
        }

        public string Get(string assignedTo)
        {
            if (string.IsNullOrEmpty(assignedTo))
                return string.Empty;

            string assignedToClause = "";
            if (assignedTo is "@me" or "me")
            {
                assignedTo = _defaultUserEmail;
            }
            if (assignedTo is "@all" or "all")
            {
                //dont include the query if we want to filter on everybody
                return "";
            }

            return $"AND  [System.AssignedTo] = '{assignedTo}'";
        }
    }
}