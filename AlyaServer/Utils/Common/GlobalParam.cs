using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;


public class ServerConfig
{
    public int ServerPort;
    public string CookieSess= "7e3c0a2b%2C1716685160%2C04d98%2Ab1CjArE7y39zinkTZxmw6RZUn_19X94A5fWRmoPR9AfyR4FxgnvYbLLFxc9OYnD4QEGIMSVnRKNk9TUUt6N0Y0cElNZlgwandLSDVtQVctdkVxd0VmR05LbkpJdUF1VDFCRlAwYWRSdzhkOUxlQ1V6QmNwZ1R2VW9iQjZRbXZQV3BlZGtqd01kRXp3IIEC";
}

public class GlobalParam 
{
    public static ServerConfig Config=new ServerConfig();

    public static void ReadFromFile()
    {
           string jsonPath = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory+"/Config/ServerSetting.json");
           Config = JsonConvert.DeserializeObject<ServerConfig>(jsonPath);
    }
}
