using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;
using Corsinvest.ProxmoxVE.Api;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Data;
using static System.Net.WebRequestMethods;
using File = System.IO.File;
using System.Net.NetworkInformation;

namespace PveAdmin;

public class Vars {

    public static string Host { get; set; } = null;
    public static int HostPort { get; set; } = 8006;
    public static string UserFullname { get; set; } = null;
    public static string UserDomain { get; set; } = null;
    public static string UserName { get; set; } = null;
    public static string UserPassword { get; set; } = null;
    public static string PveVersion { get; set; } = null;
    public static string Path { get; set; } = null;
    public static string ConfigFile { get; set; } = null;
    public static string LogFile { get; set; } = null;
    public static string NodeIp { get; set; } = null;
    public static string NodeName { get; set; } = null;
    public static string UserPasswordEntryed { get; set; } = null;
    public static string PveCookie { get; set; } = null;

    public static string PveCSRF { get; set; } = null;
    public static bool SslVerified { get; set; } = true;
    public static JObject VmList { get; set; } = null;
    public static JObject NodeList { get; set; } = null;
    public static JObject StorageList { get; set; } = null;

}

public class Options
{

    public static async Task<bool> GetResAsync()
    {
        if (Vars.UserPassword != null && Vars.UserName != null && Vars.Host != null)
        {
            var client = new PveClient(Vars.Host,Vars.HostPort);
            if (await client.Login(Vars.UserFullname, Vars.UserPassword))
            {
                var res = await client.Get("/cluster/resources?type=vm");
                string datas = JsonConvert.SerializeObject(res.ResponseToDictionary, Newtonsoft.Json.Formatting.Indented);
                Vars.VmList = JObject.Parse(datas);
                res = await client.Get("/cluster/resources?type=node");
                datas = JsonConvert.SerializeObject(res.ResponseToDictionary, Newtonsoft.Json.Formatting.Indented);
                Vars.NodeList = JObject.Parse(datas);
                res = await client.Get("/cluster/resources?type=storage");
                datas = JsonConvert.SerializeObject(res.ResponseToDictionary, Newtonsoft.Json.Formatting.Indented);
                Vars.StorageList = JObject.Parse(datas);
                Vars.PveCookie = client.PVEAuthCookie;
                Vars.PveCSRF = client.CSRFPreventionToken;
                return true;
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("测试失败", Vars.Host.ToString() + " " + client.LastResult.StatusCode.ToString(), "确定");
                return false;
            }
        }
        else {

            await Application.Current.MainPage.DisplayAlert("获取资源失败","请填写服务器和配置信息", "确定");
            return false;
        }

    }

    public static void SaveConfig()
    {

        Vars.Path = FileSystem.AppDataDirectory;
        Vars.ConfigFile = Vars.Path + "/config.json";
        if (Vars.UserPassword != null)
        {
            Vars.UserPasswordEntryed = AesEncryptHelper.EncryptAes(Vars.UserPassword);
        }

        JObject conf = new JObject
            {
                {"username",Vars.UserFullname},
                {"host", Vars.Host},
                {"port", Vars.HostPort},
                {"password",Vars.UserPasswordEntryed }
            };
        try
        {
            using (StreamWriter file = File.CreateText(Vars.ConfigFile))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, conf);
            }
            Application.Current.MainPage.DisplayAlert("提示", "保存成功", "确定");
        }
        catch {
            Application.Current.MainPage.DisplayAlert("提示", "保存失败", "确定");
        }

    }


    public static async void Load_config()
    {
        Vars.Path = FileSystem.AppDataDirectory;
        Vars.ConfigFile = Vars.Path + "/config.json";
        if (File.Exists(Vars.ConfigFile))
        {
            try
            {
                string config = File.ReadAllText(Vars.ConfigFile);
                JObject tempdata = JObject.Parse(config);
                Vars.UserFullname = tempdata["username"].ToString();
                Vars.UserPasswordEntryed = tempdata["password"].ToString();
                Vars.Host = tempdata["host"].ToString();
                Vars.HostPort = int.Parse(tempdata["port"].ToString());
                string[] UserNameParse = Vars.UserFullname.Split(new char[] { '@' });
                try
                {
                    Vars.UserName = UserNameParse[0];
                    Vars.UserDomain = UserNameParse[1];
                }
                catch {
                    await Application.Current.MainPage.DisplayAlert("提示", "用户名称不正确", "确定");
                }
                try
                {   // 尝试解密密码
                    // 如果输入框内的密码等于加密密码，
                    // 就代表这个密码是加密了的，需要重新解密
                    if (Vars.UserPasswordEntryed != null)
                    {
                        Vars.UserPassword = AesEncryptHelper.DecryptAes(Vars.UserPasswordEntryed);
                    }
                }
                catch
                {
                    await Application.Current.MainPage.DisplayAlert("提示", "密码格式不正确", "确定");
                    logging("读取密码时，密码格式不正确！");
                }
                logging("配置文件获取成功");
            }
            catch
            {
                await Application.Current.MainPage.DisplayAlert("提示", "配置文件检测失败,将重载配置文件", "确定");
                logging("配置文件检测失败,将重载配置文件！");
                SaveConfig();
                return;
            }
        }
    }

    public static void logging(string logs)
    {
        Vars.LogFile = Vars.Path + "PveAdmin.log";
        DateTime currentTime = DateTime.Now;
        using (StreamWriter file = File.AppendText(Vars.LogFile))
        {
            file.WriteLine(currentTime + " " + logs);
        }
    }

    public static async Task<string> TestServerAsync() {

        var client = new PveClient(Vars.Host,Vars.HostPort);
        string datas = "";
        try
        {
            var res = await client.Get("/version");
            datas = JsonConvert.SerializeObject(res.StatusCode);
        }
        catch {
            datas = "error";
        }

        return datas;
    }

}

public class AesEncryptHelper
{
    /// <summary>
    /// Aes加解密钥必须32位
    /// </summary>
    public static string AesKey
    {
        get
        {
            return @"GiF!KgFz+IyjGL78PU-GX9-tDJ^tw7Y^"; // 此处可自定义，32个字符长度
        }
    }

    /// <summary>
    /// 获取Aes32位密钥
    /// </summary>
    /// <param name="key">Aes密钥字符串</param>
    /// <param name="encoding">编码类型</param>
    /// <returns>Aes32位密钥</returns>
    static byte[] GetAesKey(string key, Encoding encoding)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentNullException("key", "Aes密钥不能为空");
        }
        if (key.Length < 32)
        {
            // 不足32补全
            key = key.PadRight(32, '0');
        }
        if (key.Length > 32)
        {
            key = key.Substring(0, 32);
        }
        return encoding.GetBytes(key);
    }

    /// <summary>
    /// Aes加密
    /// </summary>
    /// <param name="source">源字符串</param>
    /// <returns>加密后的字符串</returns>
    public static string EncryptAes(string source)
    {
        return EncryptAes(source, AesKey);
    }

    /// <summary>
    /// Aes加密
    /// </summary>
    /// <param name="source">源字符串</param>
    /// <param name="key">aes密钥，长度必须32位</param>
    /// <param name="model">运算模式</param>
    /// <param name="padding">填充模式</param>
    /// <param name="encoding">编码类型</param>
    /// <returns>加密后的字符串</returns>
    public static string EncryptAes(string source, string key, CipherMode model = CipherMode.ECB,
    PaddingMode padding = PaddingMode.PKCS7, Encoding encoding = null)
    {
        if (encoding == null) encoding = Encoding.UTF8;

        using (AesCryptoServiceProvider aesProvider = new AesCryptoServiceProvider())
        {
            aesProvider.Key = GetAesKey(key, encoding);
            aesProvider.Mode = model;
            aesProvider.Padding = padding;
            using (ICryptoTransform cryptoTransform = aesProvider.CreateEncryptor())
            {
                byte[] inputBuffers = encoding.GetBytes(source),
                    results = cryptoTransform.TransformFinalBlock(inputBuffers, 0, inputBuffers.Length);
                aesProvider.Clear();
                return Convert.ToBase64String(results, 0, results.Length);
            }
        }
    }

    /// <summary>
    /// Aes解密
    /// </summary>
    /// <param name="source">源字符串</param>
    /// <returns>解密后的字符串</returns>
    public static string DecryptAes(string source)
    {
        return DecryptAes(source, AesKey);
    }

    /// <summary>
    /// Aes解密
    /// </summary>
    /// <param name="source">源字符串</param>
    /// <param name="key">aes密钥，长度必须32位</param>
    /// <param name="model">运算模式</param>
    /// <param name="padding">填充模式</param>
    /// <param name="encoding">编码类型</param>
    /// <returns>解密后的字符串</returns>
    public static string DecryptAes(string source, string key, CipherMode model = CipherMode.ECB,
    PaddingMode padding = PaddingMode.PKCS7, Encoding encoding = null)
    {
        if (encoding == null) encoding = Encoding.UTF8;

        using (AesCryptoServiceProvider aesProvider = new AesCryptoServiceProvider())
        {
            aesProvider.Key = GetAesKey(key, encoding);
            aesProvider.Mode = model;
            aesProvider.Padding = padding;
            using (ICryptoTransform cryptoTransform = aesProvider.CreateDecryptor())
            {
                byte[] inputBuffers = Convert.FromBase64String(source);
                byte[] results = cryptoTransform.TransformFinalBlock(inputBuffers, 0, inputBuffers.Length);
                aesProvider.Clear();
                return encoding.GetString(results);
            }
        }
    }
}

    
