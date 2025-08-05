using Microsoft.AspNetCore.Mvc;
using System.Data;
using Dapper;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Previewer;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.ComponentModel;
using LicenseContext = OfficeOpenXml.LicenseContext;
using OfficeOpenXml.Drawing.Chart;

namespace Estacionamento.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IDbConnection _connection;

        public DashboardController(IDbConnection connection)
        {
            _connection = connection;
        }

        [HttpGet("/dashboard")]
        public IActionResult Index(string filtro = "hoje")
        {
            DateTime inicio, fim;

            switch (filtro.ToLower())
            {
                case "semana":
                    inicio = DateTime.Today.AddDays(-6);
                    fim = DateTime.Today;
                    break;
                case "mes":
                    inicio = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    fim = DateTime.Today;
                    break;
                default:
                    inicio = DateTime.Today;
                    fim = DateTime.Today;
                    break;
            }

            int totalVagas = _connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Vagas");
            int vagasOcupadas = _connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Vagas WHERE Ocupada = true");
            int ticketsAtivos = _connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Tickets WHERE DataSaida IS NULL");

            float receita = _connection.ExecuteScalar<float>(
                "SELECT COALESCE(SUM(Valor), 0) FROM Tickets WHERE DataSaida BETWEEN @inicio AND @fim",
                new { inicio = inicio, fim = fim.AddDays(1).AddSeconds(-1) });


            ViewBag.TotalVagas = totalVagas;
            ViewBag.VagasOcupadas = vagasOcupadas;
            ViewBag.TicketsAtivos = ticketsAtivos;
            ViewBag.Receita = receita;
            ViewBag.Filtro = filtro;

            var receitaPorDia = _connection.Query<(DateTime Dia, float Valor)>(
            @"SELECT DATE(DataSaida) AS Dia, 
                    SUM(Valor) AS Valor 
            FROM Tickets 
            WHERE DataSaida BETWEEN @inicio AND @fim 
            GROUP BY Dia 
            ORDER BY Dia",
            new { inicio, fim = fim.AddDays(1).AddSeconds(-1) }).ToList();

            // Cria listas separadas pro gráfico
            ViewBag.Datas = receitaPorDia.Select(r => r.Dia.ToString("dd/MM")).ToList();
            ViewBag.Valores = receitaPorDia.Select(r => r.Valor.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)).ToList();


            return View();
        }

        [HttpGet("/dashboard/exportar-pdf")]
        public IActionResult ExportarPdf(string filtro = "hoje")
        {
            QuestPDF.Settings.License = LicenseType.Community;

            DateTime inicio, fim;
            switch (filtro.ToLower())
            {
                case "semana":
                    inicio = DateTime.Today.AddDays(-6);
                    fim = DateTime.Today;
                    break;
                case "mes":
                    inicio = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    fim = DateTime.Today;
                    break;
                default:
                    inicio = DateTime.Today;
                    fim = DateTime.Today;
                    break;
            }

            var dados = _connection.Query<(DateTime Dia, float Valor)>(
                @"SELECT DATE(DataSaida) AS Dia, SUM(Valor) AS Valor 
                FROM Tickets 
                WHERE DataSaida BETWEEN @inicio AND @fim 
                GROUP BY Dia 
                ORDER BY Dia",
                new { inicio, fim = fim.AddDays(1).AddSeconds(-1) }).ToList();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);

                    page.Header().Element(container =>
                    {
                        container
                            .PaddingBottom(20)
                            .AlignCenter()
                            .Text("📊 Relatório de Receita por Dia")
                            .FontSize(24)
                            .Bold();
                    });

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                        });

                        // Cabeçalho
                        table.Header(header =>
                        {
                            header.Cell().Background("#e0e0e0").Padding(5).Text("Data").Bold();
                            header.Cell().Background("#e0e0e0").Padding(5).Text("Valor (R$)").Bold();
                        });

                        // Conteúdo
                        foreach (var item in dados)
                        {
                            table.Cell().BorderBottom(1).Padding(5).Text(item.Dia.ToString("dd/MM/yyyy"));
                            table.Cell().BorderBottom(1).Padding(5).AlignRight().Text(item.Valor.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
                        }
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text($"📅 Gerado em {DateTime.Now:dd/MM/yyyy HH:mm}")
                        .FontSize(10);
                });
            });

            var stream = new MemoryStream();
            document.GeneratePdf(stream);
            stream.Position = 0;

            return File(stream, "application/pdf", $"relatorio-receita-{filtro}.pdf");
        }

        [HttpGet("/dashboard/exportar-excel")]
        public IActionResult ExportarExcel(string filtro = "hoje")
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            DateTime inicio, fim;
            switch (filtro.ToLower())
            {
                case "semana":
                    inicio = DateTime.Today.AddDays(-6);
                    fim = DateTime.Today;
                    break;
                case "mes":
                    inicio = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    fim = DateTime.Today;
                    break;
                default:
                    inicio = DateTime.Today;
                    fim = DateTime.Today;
                    break;
            }

            var dados = _connection.Query<(DateTime Dia, float Valor)>(
                @"SELECT DATE(DataSaida) AS Dia, SUM(Valor) AS Valor 
                FROM Tickets 
                WHERE DataSaida BETWEEN @inicio AND @fim 
                GROUP BY Dia 
                ORDER BY Dia",
                new { inicio, fim = fim.AddDays(1).AddSeconds(-1) }).ToList();

            using var pacote = new ExcelPackage();
            var planilha = pacote.Workbook.Worksheets.Add("Relatório");

            // Título
            planilha.Cells["A1:B1"].Merge = true;
            planilha.Cells["A1"].Value = "Relatório de Receita por Dia";
            planilha.Cells["A1"].Style.Font.Size = 16;
            planilha.Cells["A1"].Style.Font.Bold = true;
            planilha.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // Cabeçalho
            planilha.Cells["A2"].Value = "Data";
            planilha.Cells["B2"].Value = "Valor (R$)";
            planilha.Cells["A2:B2"].Style.Font.Bold = true;
            planilha.Cells["A2:B2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            planilha.Cells["A2:B2"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            planilha.Cells["A2:B2"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

            int linha = 3;
            foreach (var item in dados)
            {
                planilha.Cells[linha, 1].Value = item.Dia.ToString("dd/MM/yyyy");
                planilha.Cells[linha, 2].Value = item.Valor;
                planilha.Cells[linha, 2].Style.Numberformat.Format = "R$ #,##0.00";
                linha++;
            }

            planilha.Cells.AutoFitColumns();

            // Adiciona gráfico
            var chart = planilha.Drawings.AddChart("graficoReceita", eChartType.ColumnClustered);
            chart.Title.Text = "Receita por Dia";
            chart.SetPosition(linha + 1, 0, 0, 0); // posição abaixo da tabela
            chart.SetSize(600, 400);

            var dataRange = planilha.Cells[$"B3:B{linha - 1}"];
            var labelRange = planilha.Cells[$"A3:A{linha - 1}"];

            var serie = chart.Series.Add(dataRange, labelRange);
            serie.Header = "Valor (R$)";

            var stream = new MemoryStream();
            pacote.SaveAs(stream);
            stream.Position = 0;

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"relatorio-receita-{filtro}.xlsx");
        }

    }
}
