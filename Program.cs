using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace EOE日程抓取加链接
{
    internal class Program
    {
        static void Main(string[] args) 
        {
        Program a = new Program();
            a.sb();
            while (true) { 
           Thread.Sleep(1000);
            }
        }
       public async void sb()
        {
            Console.WriteLine("Hello, World!");
            get_schdule gs = new get_schdule();
            Bot_start bt = new Bot_start(gs);
            bt.run();
            while (true) 
            {
            update_schdule:
                int i = 0;
                try { 
                    await gs.get_new_dongtai();
                    Console.WriteLine("更新成功");
                }
                catch 
                {
                    i = i + 1;
                    if (i < 4)
                    {
                        Console.WriteLine("更新失败，重试");
                        await Task.Delay(1000);//延时
                        goto update_schdule;
                    }
                    else
                        Console.WriteLine("多次失败，停止尝试");
                  }
              await Task.Delay(1000*60*30);               
            }
            

        }
    }
    public class Bot_start
    {
        
        public TelegramBotClient bot;

       
        private get_schdule gs;
        public Bot_start(get_schdule gs)
        {
            this.bot = new TelegramBotClient("6855135700:AAGfOFMePaRRQe7MjkXh11Nt2NgqJciHGu8");
            bot.Timeout = TimeSpan.FromSeconds(10);
            this.gs = gs;
        }
        public async void run()
        {

            var me = await bot.GetMeAsync();
            Console.WriteLine(me.Id.ToString() + "bot启动");
        startreceive:
            try
            {
                bot.StartReceiving(Update, error);
                Console.WriteLine("启动接受成功");
            }
            catch (Exception a)
            {
                Console.WriteLine("启动接收失败，再次尝试.错误信息为" + a.Message.ToString());
                goto startreceive;
            }

        }

        
        
        private Task error(ITelegramBotClient client, Exception exception, CancellationToken token)
        {
            Console.WriteLine($"Error: {exception.Message}" + " TASKerror");
            Thread.SpinWait(3);
            return Task.CompletedTask;
        }

        private async Task Update(ITelegramBotClient client, Update update, CancellationToken token)
        {
        p2:

            try
            {
#nullable enable
                Message m = update.Message;
                if (m == null )
                {
                    Console.WriteLine("无消息");
                }                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             
                else
                {
                  await  process(m);

                }

            }
            catch (Exception a)
            {

                Console.WriteLine("2", a.Message);
                goto p2;
            }
        



        }
        public async Task process(Message m)
        {
          
            
            //判断是否过期，界限五分钟

           DateTime message_time=m.Date;
           DateTime now_time=DateTime.Now;
          
           // Console.WriteLine(message_time.ToString());
            //Console.WriteLine(now_time.ToString());
            if ((now_time.AddHours(-8) - message_time).TotalMinutes > 5)
            {
            //Console.Write(message_time.ToString("过期信息，不处理"));
            }
            else
            {
                process_body body;

                body = get_body_info(m);
                Console.WriteLine("开始处理1");
                if (Contain_order(body))
                {


                    Console.WriteLine("开始处理2");


                    await Sendmessage(body, bot);
                }


            }

        }
        //获取发送者id,发送文本
        private process_body get_body_info(Message m)
        {
           
            process_body body;
            body.m = m;
            body.received_text = m.Text;
            body.schdule = gs.get_exit_Schdule();
                  
            return body;          
        }
        //发送信息
        public async Task Sendmessage(process_body body,TelegramBotClient bot)            
        {
            tryagain:
            try {
                Console.WriteLine("开始发送");

                await
                    bot.SendTextMessageAsync(
                        chatId:body.m.Chat.Id,
                        text:body.schdule.description,
                        replyToMessageId: body.m.MessageId,
                        disableNotification: true,
                        parseMode: ParseMode.Html);
                   /* bot.SendPhotoAsync(
                chatId: body.id,
                photo: body.schdule.image_url),
                caption: body.schdule.description,
                replyToMessageId:body.messageid,
                disableNotification: true,
                parseMode: ParseMode.Html) ;*/
           
                Console.WriteLine("发送成功"); 
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException ex) { 
                Console.WriteLine("发送失败");
                Console.WriteLine($"Telegram API request failed: {ex.Message}");

                
                goto tryagain;
            }
            
            
        }
        //检测发送文本是否包含命令
        private bool Contain_order(process_body body)
        {
            if (body.received_text == null)
            {
                Console.WriteLine("文本为空");
                return false;
            }
            else if(body.received_text.Equals("日程") | body.received_text.Equals("/日程")) 
            {
                Console.WriteLine("接受到指令");
                return true;  }
           return false;
            
            
        }
       
    }
   
}