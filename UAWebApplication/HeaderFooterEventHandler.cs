
using iText.Kernel.Events;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;

namespace UAWebApplication
{
    public class HeaderFooterEventHandler : iText.Kernel.Events.IEventHandler
    {
        private Table header_table;
        private Table footer_table;
        private float Top;
        private float Left;
        private float Right;
        private float Bottom;

        public HeaderFooterEventHandler(Table header_table,Table footer_table,
            float Top,float Left,float Right,float Bottom)
        {
            this.header_table = header_table;
            this.footer_table = footer_table;
            this.Top = Top;
            this.Left = Left;
            this.Right = Right;
            this.Bottom = Bottom;
        }

        public void HandleEvent(Event @event)
        {
            PdfDocumentEvent documentEvent = (PdfDocumentEvent)@event;
            PdfDocument pdfDoc = documentEvent.GetDocument();
            PdfPage page = documentEvent.GetPage();
            Rectangle pageSize = page.GetPageSize();

            PdfCanvas canvas = new PdfCanvas(page.NewContentStreamBefore(), page.GetResources(), pdfDoc);

            // draw the table on the PDF canvas
            header_table.SetFixedPosition(pageSize.GetLeft() + Left, pageSize.GetTop() - Top, pageSize.GetWidth() - (Left+Right));
            header_table.SetBorder(Border.NO_BORDER);
            new Canvas(canvas, pageSize).Add(header_table);

            // draw the table on the PDF canvas
            footer_table.SetFixedPosition(pageSize.GetLeft() + Left, pageSize.GetBottom() + Bottom, pageSize.GetWidth() - (Left + Right));
            footer_table.SetBorder(Border.NO_BORDER);
            new Canvas(canvas, pageSize).Add(footer_table);

            canvas.Release();
        }
    }

}
