﻿using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenCredentialPublisher.Credentials.Drawing
{
    public class PdfUtility
    {
        public const string PageOutlineBase = "OpenCredentialPublisher-QRCode-";
        public const string PageOutlineTitle = PageOutlineBase + "PublishingService";
        public static byte[] AppendQRCodePage(byte[] pdfBytes, string url, string sourceApplicationName)
        {
            MyFontResolver.Apply();

            System.Text.Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using var stream = new MemoryStream(pdfBytes, false);
            using var document = PdfReader.Open(stream, PdfDocumentOpenMode.Modify);
            var page = document.AddPage();
            var graphics = XGraphics.FromPdfPage(page);

            document.Outlines.Add(new PdfSharp.Pdf.PdfOutline(PageOutlineBase + sourceApplicationName, page));

            const double margin = 10;

            var titleFont = new XFont("Verdana", 20);
            graphics.DrawString($"Use one of the methods below to verify", titleFont,
                XBrushes.Black, new XRect(0, 5, page.Width, 30),
                XStringFormats.Center);

            graphics.DrawString($"the authenticity of this document.", titleFont,
                XBrushes.Black, new XRect(0, 35, page.Width, 30),
                XStringFormats.Center);

            var pen = new XPen(XColors.Black, 2)
            {
                DashStyle = XDashStyle.Solid
            };
            graphics.DrawLine(pen, margin, 70, page.Width - margin, 70);

            var pageFont = new XFont("Verdana", 16);

            graphics.DrawString("Using your mobile device, scan the QR Code below", pageFont,
                XBrushes.Black, new XRect(0, 100, page.Width, 20),
                XStringFormats.Center);

            var qrCodeBytes = QRCodeUtility.Create(url);

            using var qrCodeStream = new MemoryStream(qrCodeBytes, false);
            var qrCodeImage = XImage.FromStream(qrCodeStream);
            // 385x385

            double width = qrCodeImage.PixelWidth * 72 / qrCodeImage.HorizontalResolution;
            double height = qrCodeImage.PixelHeight * 72 / qrCodeImage.VerticalResolution;

            var qrCodeY = 140;
            graphics.DrawImage(qrCodeImage, (page.Width - width) / 2, qrCodeY, width, height);

            var separatorY = qrCodeY + height + 40;
            graphics.DrawString("OR", titleFont,
                XBrushes.Black, new XRect(0, separatorY, page.Width, 20),
                XStringFormats.Center);

            var separatorLineY = separatorY + 10;
            graphics.DrawLine(pen, margin, separatorLineY, (page.Width / 2) - 20, separatorLineY);
            graphics.DrawLine(pen, (page.Width / 2) + 20, separatorLineY, page.Width - margin, separatorLineY);

            var clickTextY = separatorLineY + 40;
            graphics.DrawString($"Click the following link", pageFont,
                XBrushes.Black, new XRect(0, clickTextY, page.Width, 20),
                XStringFormats.Center);


            var displayUrl = url;
            if (url.Length > 93)
            {
                var splitLocation = displayUrl.IndexOf("/", displayUrl.IndexOf("//") + 2);
                displayUrl = displayUrl.Substring(0, splitLocation);
            }
            var urlY = clickTextY + 30;
            var urlFont = new XFont("Verdana", 12, XFontStyle.Underline);
            var weblinkRect = new XRect(5, urlY - 400, page.Width - 5, 60);
            var pdfRectangle = new PdfRectangle(weblinkRect);

            page.AddWebLink(pdfRectangle, url);
            var urlRect = new XRect(5, urlY, page.Width - 5, 60);
            graphics.DrawString(displayUrl, urlFont,
                XBrushes.Blue, urlRect, XStringFormats.Center);

            //var accessKeyY = urlY + 20;
            //graphics.DrawString($"Access Key: {accessKey}", pageFont,
            //    XBrushes.Black, new XRect(margin, accessKeyY, page.Width, 20),
            //    XStringFormats.CenterLeft);
            graphics.Save();

            using var saveStream = new MemoryStream();
            document.Save(saveStream);
            return saveStream.ToArray();
        }

        public static byte[] RemoveExistingQRCode(byte[] pdfBytes, string sourceApplicationName)
        {
            MyFontResolver.Apply();

            System.Text.Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using var stream = new MemoryStream(pdfBytes, false);
            using var document = PdfReader.Open(stream, PdfDocumentOpenMode.Modify);
            if (document.Outlines.Any())
            {
                var outlines = document.Outlines.Where(o => o.Title.Contains(PageOutlineBase));
                foreach (var outline in outlines)
                {
                    if (outline.Title != (PageOutlineBase + sourceApplicationName))
                    {
                        document.Pages.Remove(outline.DestinationPage);
                    }
                }
            }
            using var saveStream = new MemoryStream();
            document.Save(saveStream);
            return saveStream.ToArray();
        }
    }
}
