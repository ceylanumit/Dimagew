using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using SkiaSharp;


namespace Dimagew
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private void AddImagesToListBox(List<string> imageUrls)
        {
            foreach (string imageUrl in imageUrls)
            {
                listBox1.Items.Add(imageUrl);
            }
        }

        private List<string> GetImageUrlsFromUrl(string url)
        {
            List<string> imageUrls = new List<string>();

            try
            {
               
                WebClient webClient = new WebClient();
                string htmlContent = webClient.DownloadString(url);

               
                MatchCollection matches = Regex.Matches(htmlContent, "<img.+?src=[\"'](.+?)[\"'].*?>", RegexOptions.IgnoreCase);
                foreach (Match match in matches)
                {
                    string imageUrl = match.Groups[1].Value;
                    imageUrls.Add(imageUrl);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata oluştu: " + ex.Message);
            }

            return imageUrls;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string url = textBox1.Text;
            List<string> imageUrls = GetImageUrlsFromUrl(url);
            AddImagesToListBox(imageUrls);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string url = textBox1.Text;

            try
            {
                List<string> imageUrls = GetImageUrlsFromUrl(url);

                foreach (string imageUrl in imageUrls)
                {
                    WebClient webClient = new WebClient();
                    byte[] imageData = webClient.DownloadData(imageUrl);

                    string fileName = Path.GetFileName(new Uri(imageUrl).AbsolutePath);

                    
                    string fileExtension = Path.GetExtension(fileName);

                    
                    string extension = GetImageExtension(imageData);

                    
                    fileName = Path.GetFileNameWithoutExtension(fileName) + "." + extension;

                    string savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), fileName);
                    File.WriteAllBytes(savePath, imageData);
                }

                MessageBox.Show("Tüm resimler başarıyla indirildi.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bazı Hatalar Var: " + ex.Message + " Resimler klasörünüzü kontrol edin.");
            }
        }

        private string GetImageExtension(byte[] imageData)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream(imageData))
                {
              
                    stream.Seek(0, SeekOrigin.Begin);

                    using (Image image = Image.FromStream(stream, false, false))
                    {
                      
                        if (image.PixelFormat == System.Drawing.Imaging.PixelFormat.Undefined)
                            throw new ArgumentException("Resim verisi geçerli bir görüntü biçimine sahip değil.");

                        if (IsWebPImage(imageData))
                            return "webp";
                        if (image.RawFormat.Equals(ImageFormat.Jpeg))
                            return "jpg";
                        else if (image.RawFormat.Equals(ImageFormat.Png))
                            return "png";
                        else if (image.RawFormat.Equals(ImageFormat.Gif))
                            return "gif";
                        else if (image.RawFormat.Equals(ImageFormat.Bmp))
                            return "bmp";
                        else if (image.RawFormat.Equals(ImageFormat.Icon))
                            return "ico";
                        else
                            return "jpg";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Hata: " + ex.Message + " Resimler klasörünüzü kontrol edin.");
                return "jpg"; 
            }
        }

        private bool IsWebPImage(byte[] imageData)
        {
            byte[] webPHeader = new byte[] { 0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50 };
            for (int i = 0; i < webPHeader.Length; i++)
            {
                if (imageData.Length <= i || imageData[i] != webPHeader[i])
                    return false;
            }
            return true;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedImageUrl = listBox1.SelectedItem as string;
            if (selectedImageUrl != null)
            {
                try
                {
                    WebClient webClient = new WebClient();
                    byte[] imageData = webClient.DownloadData(selectedImageUrl);

                    pictureBox1.Image = LoadImage(imageData);
                    pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage; 
                    pictureBox1.Dock = DockStyle.Fill;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hata oluştu: " + ex.Message);
                }
            }
        }

        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        private Image LoadImage(byte[] imageData)
        {
            using (MemoryStream stream = new MemoryStream(imageData))
            {
                using (SKBitmap skBitmap = SKBitmap.Decode(stream))
                {
                    using (SKImage skImage = SKImage.FromBitmap(skBitmap))
                    {
                        using (SKPixmap skPixmap = skImage.PeekPixels())
                        {
                            SKImageInfo skInfo = new SKImageInfo(skPixmap.Width, skPixmap.Height);
                            using (SKBitmap result = new SKBitmap(skInfo))
                            {
                                if (skPixmap.ReadPixels(skInfo, result.GetPixels(), skInfo.RowBytes, 0, 0))
                                {
                                    using (MemoryStream memoryStream = new MemoryStream())
                                    {
                                        using (SKImage resultImage = SKImage.FromBitmap(result))
                                        {
                                            resultImage.Encode(SKEncodedImageFormat.Png, 100).SaveTo(memoryStream);
                                            return Image.FromStream(memoryStream);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        private void button2_Click(object sender, EventArgs e)
        {
           
            if (listBox1.SelectedItems.Count > 0)
            {
                foreach (string selectedImageUrl in listBox1.SelectedItems)
                {
                    try
                    {
                        WebClient webClient = new WebClient();
                        byte[] imageData = webClient.DownloadData(selectedImageUrl);

                        string fileName = Path.GetFileName(new Uri(selectedImageUrl).AbsolutePath);
                        string extension = GetImageExtension(imageData);
                        fileName = Path.GetFileNameWithoutExtension(fileName) + "." + extension;

                        string savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), fileName);
                        File.WriteAllBytes(savePath, imageData);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Hata oluştu: " + ex.Message);
                    }
                }

                MessageBox.Show("Seçilen tüm resimler başarıyla indirildi", "Sistem Mesajı");
            }
            else
            {
                MessageBox.Show("Lütfen bir veya daha fazla resim seçin.", "Sistem Mesajı");
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }
    }
}

    
   

