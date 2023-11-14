
using Newtonsoft.Json.Linq;
using System;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Telegram.Bot.Types;
using System.Text.Json;
using EOE日程抓取加链接;
using Telegram.Bot;
using System.Data;

/// <summary>
/// Summary description for Class1
/// </summary>
/// 

public class get_schdule
{
    Schdule tem_schdule;
	string get_schdule_url;
	HttpClient httpClient;
    
	public get_schdule() 
	{
		get_schdule_url= $"https://rss-hub-three.vercel.app/bilibili/user/dynamic/2018113152";
        httpClient = new HttpClient();
    }
	
    public Schdule get_exit_Schdule()
    {
        Console.WriteLine("开始处理");
        Schdule s=excel_read();
		return s;
    }



    //获取本地保存的日程信息
    

    
   //判断最新一条是否为日程,如果是则初始化s并返回true
	public async Task<Task> get_new_dongtai() 
	{
		string xml_response = await httpClient.GetStringAsync(get_schdule_url);
		if (xml_response == null)
		{
			Console.WriteLine("client获取失败");
            return Task.CompletedTask;
		}
		else 
		{
			Console.WriteLine("client获取成功");
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(xml_response);
			XmlNodeList root =xmlDocument.SelectNodes("//item");
            foreach (XmlNode item_xnod in root)
            {
                string text_describe = item_xnod.SelectSingleNode("description").InnerText;
                if (text_describe.Contains("直播安排"))
                {
                    tem_schdule.description = remove_image(text_describe);

                    tem_schdule.pubDate = item_xnod.SelectSingleNode("pubDate").InnerText;
                    if (not_have_exit(tem_schdule))
                    {
                        Console.WriteLine("更新日程");
                        
                        return Task.CompletedTask;
                    }
                    else
                        Console.WriteLine("无新发日程");
                    return Task.CompletedTask;

                }
            }

            return Task.CompletedTask;
        }

		
	}
    //移除imageurl，并替换《br》为换行符
    private string remove_image( string s)
    {
              

        if (s!=null)
        {
            // 从字符串中删除其他<img>标签
            string pattern = @"<img\b[^>]*>";
            s =Regex.Replace(s,pattern,string.Empty);
            //替换<br>为换行符
            s=s.Replace("<br>", "\r\n");
            //添加超链接
            Console.WriteLine("开始添加链接");
            s=addlink(s);
            Console.WriteLine($"Modified String: {s}");
			return s;
        }
        else
        {
            Console.WriteLine("No <img> tag found in the input string.");
			return "";
        }
		
    }
   private string addlink(string s)
    {
        string jsonText = System.IO.File.ReadAllText("dictionary.json");
        if (jsonText == null)
            Console.WriteLine("空json");
        else
        {
            Dictionary<string, string>? wordUrls = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonText);
            foreach (var wordUrl in wordUrls)
            {
                string word = wordUrl.Key;
                string url = wordUrl.Value;


                int index = s.IndexOf(word);


                if (index >= 0)
                {
                    // 构建超链接的HTML标记
                    string linkTag = $"<a href=\"{url}\">{word}</a>";
                    // 将要替换的单词替换为超链接
                    s = s.Replace(word, linkTag);
                }

            }
        }
       
        return s;
    }
    private bool not_have_exit(Schdule s)
	{
		
		Schdule exit_s= excel_read();
		if (exit_s.pubDate.EndsWith(s.pubDate))
		{
			return false;
		}
		else
		{
            excel_save(s);
			return true;

        }
     
	}
    private Schdule excel_read()
    {
		Console.WriteLine("开始读取");
		XDocument doc = XDocument.Load("schdule.xml");
		XElement item_element = doc.Root;
		Schdule s = new Schdule
		{
			description = item_element.Element("description").Value,
			pubDate= item_element.Element("pubDate").Value,
        };
        DateTime now_time =DateTime.Now;
        string date = now_time.ToString("MM-dd HH:mm");
        string dateofweek = now_time.ToString("dddd");
        s.description = $"<b>现在为{date}\n{dateofweek}</b>\n{s.description}";
        Console.WriteLine("读取完成");
        return s;
    }
    //保存日程信息到本地
    private void excel_save(Schdule schdule)
    {
		XmlSerializer serializer=new XmlSerializer(typeof(Schdule));
        using (FileStream fileStream = new FileStream("schdule.xml", FileMode.Create))
        {
            serializer.Serialize(fileStream, schdule);
            Console.WriteLine("XML data has been written to the file.");
        }
       
    }
}
