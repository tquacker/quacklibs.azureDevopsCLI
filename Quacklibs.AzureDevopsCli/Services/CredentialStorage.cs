using System.Text.Json;


namespace Quacklibs.AzureDevopsCli.Services
{
    public interface ICredentialStorage
    {
       // string GetCredential(string username);
       // void SetCredential(PersonalAccessToken pat);

       // void Delete();
    }

    internal class CredentialStorage : ICredentialStorage
    {
        private readonly string tokenFilePath;

        public CredentialStorage()
        {
            this.tokenFilePath = this.GetTokenFile();
        }

        public void Clear()
        {
            if (File.Exists(this.tokenFilePath))
            {
                File.Delete(this.tokenFilePath);
            }
        }

        public void Delete()
        {
            if (File.Exists(this.tokenFilePath))
            {
                File.Delete(this.tokenFilePath);
                Console.WriteLine("Credentials deletet");
            }
            else
            {
                Console.WriteLine("No credentials found to delete");
            }
        }

        public string GetCredential(string username)
        {
            string result = null;

            try
            {
                if (File.Exists(this.tokenFilePath))
                {
                    var credentialsJson = File.ReadAllText(this.tokenFilePath);

                    //TODO: Safe storage
                    //var protetedContentBytes = Convert.FromBase64String(protectedContentBytesBase64);
                    //var contentBytes = ProtectedData.Unprotect(protetedContentBytes, null, DataProtectionScope.CurrentUser);
                    //var jsonContent = Encoding.UTF8.GetString(contentBytes);

                   
                    var credentials = JsonSerializer.Deserialize<Credentials>(credentialsJson);

                    result = credentials.PersonalAccessToken;
                }
            }
            catch
            {
                this.Clear();
            }

            return result;
        }

        public void SetCredential(PersonalAccessToken pat)
        {
            try
            {
                string jsonContent = JsonSerializer.Serialize(pat);
               
                //TODO: store this stuff somewhere safe
                // var contentBytes = Encoding.UTF8.GetBytes(jsonContent);

                //var protectedContentBytes = passwo.Protect(contentBytes, null, DataProtectionScope.CurrentUser);
                //var protectedContentBytesBase64 = Convert.ToBase64String(protectedContentBytes);
                File.WriteAllText(this.tokenFilePath, jsonContent);
            }
            catch (PlatformNotSupportedException)
            {
                Debug.WriteLine("Could not store credentials");
            }
        }

        private string GetTokenFile()
        {
            string homeUserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Join(homeUserProfile, ".devops", "token.bin");
        }

        private class Credentials
        {
            public string PersonalAccessToken { get; set; }

        }
    }
}
