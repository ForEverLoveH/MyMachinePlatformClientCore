 
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Brush = System.Windows.Media.Brush;
using Color = System.Drawing.Color;
using FontStyle = System.Drawing.FontStyle;
using HandyControl.Controls;
using MyMachinePlatformClientCore.Managers;


namespace MyMachinePlatformClientCore.ViewModels;

 /// <summary>
 /// 
 /// </summary>
 public class LoginWindowViewModel: BindableBase
 {
    /// <summary>
    /// 
    /// </summary>
    private CMachineManager machineManagers;
    /// <summary>
    /// 登录窗口视图模型
    /// </summary>
    /// <param name="managers"></param>
    /// <param name="userService"></param>
     public LoginWindowViewModel(CMachineManager machineManager)
     {
         this.machineManagers = machineManager;
        // this.userService = userService;

        CreateVerficationImage(200, 200);
        machineManagers.Init();

     }

     #region  绑定信息
     
     private string userName;
     /// <summary>
     /// 
     /// </summary>
     public string UserName
     {
         get { return userName; }
         set { SetProperty(ref userName, value); }
     }
     private string password;

     public string Password
     {
         get { return password; }
         set { SetProperty(ref password, value); }
     }
     private string verficationCode;
     /// <summary>
     /// 
     /// </summary>
     public string VerficationCode
     {
         get { return verficationCode; }
         set { SetProperty(ref verficationCode, value); }
     }
     private BitmapImage verficationCodeImage;
     /// <summary>
     /// 
     /// </summary>
     public BitmapImage VerficationCodeImage
     {
         get => verficationCodeImage; set { SetProperty(ref verficationCodeImage, value); }
     }
     private DelegateCommand<object> loginCommand;
     /// <summary>
     /// 当前验证码
     /// </summary>
     private string currentVerficationCode;
     /// <summary>
     /// 
     /// </summary>
     public DelegateCommand<object> LoginCommand
     {
         get =>
         loginCommand ??= new DelegateCommand<object>(async (obj) =>
         {
             if (string.IsNullOrEmpty(this.userName) || string.IsNullOrEmpty(this.password) || string.IsNullOrEmpty(this.verficationCode))
             {
                 HandyControl.Controls.MessageBox.Show("用户名、密码和验证码不能为空");
                 return;
             }
             else
             {
                 if(currentCode!=verficationCode)
                 {
                     HandyControl.Controls.MessageBox.Show("验证码错误,请重新输入！！");
                     return;
                 }
                 else
                 {
                    var res =  await machineManagers.LoginAsync(userName, password);
                 }
             }

         });
     }
    private string currentCode = "";
    /// <summary>
    /// 
    /// </summary>
    private DelegateCommand<object> refreshVerficationCodeCommand;
    /// <summary>
    /// 
    /// </summary>
    public DelegateCommand<object> RefreshVerficationCodeCommand
    {
        get => refreshVerficationCodeCommand ??= new DelegateCommand<object>(async (obj) =>
        {
            CreateVerficationImage(200, 200);
        });
        
    }



     #endregion

    /// <summary>
    /// 
    /// </summary>
     private void CreateVerficationImage(int hei,int wei)
     {
         string txt = GenerateRandomTxt();
         currentCode = txt;
         //this.verficationCode = txt;
         string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images/5.png");
         byte[] img = GenerateCatchaImage(txt, hei, wei,path);
         BitmapImage image = new BitmapImage();
         image.BeginInit();
         image.StreamSource = new MemoryStream(img);
         image.EndInit(); 
         this.VerficationCodeImage = image;
     }
     
     /// <summary>
     /// 
     /// </summary>
     /// <returns></returns>
     private string GenerateRandomTxt()
     {
         Random random = new Random();
         char[] chars = new char[6]; // 创建一个长度为6的字符数组来存储结果
 
         for (int i = 0; i < 6; i++)
         {
             // 随机决定是生成数字还是字母
             if (random.Next(2) == 0) // 0或1，50%的概率生成数字，50%的概率生成字母
             {
                 // 生成数字0-9
                 chars[i] = (char)('0' + random.Next(10)); // '0'是字符'0'的ASCII码，加上一个0-9的随机数得到0-9的字符
             }
             else
             {
                 // 生成大写字母A-Z
                 chars[i] = (char)('A' + random.Next(26)); // 'A'是字符'A'的ASCII码，加上一个0-25的随机数得到A-Z的字符
             }
         }
 
         // 将字符数组转换为字符串并打印
         string result = new string(chars);
         return result;
     }

     /// <summary>
     /// 
     /// </summary>
     /// <param name="txt"></param>
     /// <param name="hei"></param>
     /// <param name="wei"></param>
     /// <returns></returns>
     private byte[] GenerateCatchaImage(string txt,int hei,int wei,string backGroundImagePath = "")
     {
        using (Bitmap bitmap = new Bitmap(wei, hei))
        {
            
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                using (System.Drawing.Image backGroundImage = System.Drawing.Image.FromFile(backGroundImagePath))
                {                   
                     g.DrawImage(backGroundImage,  new Rectangle(0, 0, wei, hei));      
                }
                g.Clear(Color.White);
                Font font = new Font("Arial", 20, FontStyle.Bold);
                SolidBrush brush = new SolidBrush(Color.Black);
                SizeF txtSize = g.MeasureString(txt, font);
                float x = (wei - txtSize.Width) / 2;
                float y = (hei - txtSize.Height) / 2;
                g.DrawString(txt, font, brush, x, y); // 绘制文字
                using (MemoryStream stream = new MemoryStream())
                {
                    bitmap.Save(stream, ImageFormat.Png);
                    return stream.ToArray();
                }

            }
        }
    }
 }