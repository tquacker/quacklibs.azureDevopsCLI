namespace Quacklibs.AzureDevopsCli.Core.Types
{
    public class WiqlQuery
    {
        private readonly string _rawQuery;

        public WiqlQuery(string rawQuery)
        {
            _rawQuery = rawQuery;
        }

        private void Clean()
        {
            var cleanedQuery = string.Join(
                Environment.NewLine,
                _rawQuery
                    .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
                    .Select(line => line.Trim())
                    .Where(line => !string.IsNullOrWhiteSpace(line)));
        }


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