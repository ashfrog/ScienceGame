using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PrintPhoto
{
    public partial class Form1 : Form
    {
        private string imgpath = "";
        public Form1()
        {
            //隐藏窗口
            this.ShowInTaskbar = false;
            this.WindowState = FormWindowState.Minimized;

            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //参数传的图片路径
            string[] startupArgs = Environment.GetCommandLineArgs();
            for (int i = 1; i < startupArgs.Length; i++)
            {
                imgpath = startupArgs[i];
                if (File.Exists(imgpath))
                {
                    PrintPhoto();
                }
            }
            //添加打印队列后退出程序
            System.Environment.Exit(0);
        }

        private void PrintPhoto()
        {
            // 创建一个PrintDocument对象
            PrintDocument printDocument = new PrintDocument();

            printDocument.PrintController = new StandardPrintController();

            // 设置为横向打印
            printDocument.DefaultPageSettings.Landscape = true;

            // 设置PrintPage事件处理程序
            printDocument.PrintPage += new PrintPageEventHandler(PrintImage);

            // 打印照片
            printDocument.Print();
        }

        private void PrintImage(object sender, PrintPageEventArgs e)
        {
            // 加载要打印的照片
            Image image = Image.FromFile(imgpath);

            // 检查图片是否需要旋转（竖版转横版）
            Image processedImage = ProcessImageForPrint(image);
            image.Dispose();

            // 获取打印页面的大小
            Rectangle printableArea = new Rectangle(0, 0, e.PageBounds.Width, e.PageBounds.Height);

            // 将照片绘制到打印页面上，保持宽高比并居中
            DrawImageCentered(e.Graphics, processedImage, printableArea);

            // 释放资源
            processedImage.Dispose();

            //删除已添加打印的文件
            File.Delete(imgpath);
        }

        /// <summary>
        /// 处理图片用于打印（竖版转横版）
        /// </summary>
        /// <param name="originalImage"></param>
        /// <returns></returns>
        private Image ProcessImageForPrint(Image originalImage)
        {
            // 如果图片是竖版的（高度大于宽度），则旋转90度
            if (originalImage.Height > originalImage.Width)
            {
                return RotateImage(originalImage, 90);
            }

            // 如果已经是横版，直接返回副本
            return new Bitmap(originalImage);
        }

        /// <summary>
        /// 旋转图片
        /// </summary>
        /// <param name="image">原图片</param>
        /// <param name="angle">旋转角度</param>
        /// <returns>旋转后的图片</returns>
        private Image RotateImage(Image image, float angle)
        {
            // 创建新的bitmap，尺寸交换
            Bitmap rotatedBitmap = new Bitmap(image.Height, image.Width);

            using (Graphics graphics = Graphics.FromImage(rotatedBitmap))
            {
                // 设置高质量渲染
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                // 移动到中心点
                graphics.TranslateTransform(rotatedBitmap.Width / 2, rotatedBitmap.Height / 2);

                // 旋转
                graphics.RotateTransform(angle);

                // 绘制图片（从中心点开始）
                graphics.DrawImage(image, -image.Width / 2, -image.Height / 2);
            }

            return rotatedBitmap;
        }

        /// <summary>
        /// 拉伸图片填满整个画布
        /// </summary>
        /// <param name="graphics">绘图对象</param>
        /// <param name="image">要绘制的图片</param>
        /// <param name="bounds">绘制区域</param>
        private void DrawImageCentered(Graphics graphics, Image image, Rectangle bounds)
        {
            // 设置高质量渲染
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

            // 直接拉伸图片填满整个打印区域
            graphics.DrawImage(image, bounds);
        }
    }
}