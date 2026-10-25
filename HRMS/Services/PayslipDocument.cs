using System;
using System.Globalization;
using System.Linq;
using HRMS.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HRMS.Services;

public class PayslipDocument(PayrollDetailDto detail, PayrollRunSummaryDto run, string companyName = "Human Resource Management System Ltd.") : IDocument
{
    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
    public DocumentSettings GetSettings() => DocumentSettings.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(30);
            page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Segoe UI"));

            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer().Element(ComposeFooter);
        });
    }

    private void ComposeHeader(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(titleCol =>
                {
                    titleCol.Item().Text(companyName).FontSize(16).Bold().FontColor(Colors.Indigo.Darken2);
                    titleCol.Item().Text("Corporate Headquarters • 123 Tech Avenue, Banani, Dhaka").FontSize(8).FontColor(Colors.Grey.Darken1);
                    titleCol.Item().Text("CONFIDENTIAL SALARY PAYSLIP").FontSize(10).SemiBold().FontColor(Colors.Grey.Darken3);
                });

                row.ConstantItem(150).AlignRight().Column(metaCol =>
                {
                    var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(run.Month);
                    metaCol.Item().Text($"{monthName} {run.Year}").FontSize(14).Bold().FontColor(Colors.Indigo.Medium);
                    metaCol.Item().Text($"Payslip Ref: PAY-{run.Year}-{run.Month:D2}-{detail.Id:D4}").FontSize(8).FontColor(Colors.Grey.Darken1);
                    metaCol.Item().Text($"Generated: {DateTime.UtcNow:dd-MMM-yyyy}").FontSize(8).FontColor(Colors.Grey.Darken1);
                });
            });

            col.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Indigo.Lighten3);
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingTop(15).Column(col =>
        {
            // Employee & Attendance Info Boxes
            col.Item().Row(row =>
            {
                // Employee Details
                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(empCol =>
                {
                    empCol.Item().Text("EMPLOYEE DETAILS").FontSize(9).Bold().FontColor(Colors.Indigo.Darken1);
                    empCol.Item().PaddingTop(4).Text($"Employee Name: {detail.EmployeeName}").SemiBold();
                    empCol.Item().Text($"Employee Code: {detail.EmployeeCode}");
                    empCol.Item().Text($"Department: {detail.DepartmentName ?? "N/A"}");
                    empCol.Item().Text($"Designation: {detail.DesignationTitle ?? "N/A"}");
                    empCol.Item().Text($"Bank: {detail.BankName ?? "Eastern Bank PLC"}");
                    empCol.Item().Text($"Account: {detail.AccountNumber ?? "1041009876543"}");
                });

                row.ConstantItem(15);

                // Attendance Metrics
                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(attCol =>
                {
                    attCol.Item().Text("ATTENDANCE & WORK DAYS").FontSize(9).Bold().FontColor(Colors.Indigo.Darken1);
                    attCol.Item().PaddingTop(4).Text($"Total Working Days: {detail.WorkingDays} days").SemiBold();
                    attCol.Item().Text($"Days Present: {detail.PresentDays} days");
                    attCol.Item().Text($"Paid Leave: {detail.PaidLeaveDays} days");
                    attCol.Item().Text($"Unpaid Leave (LOP): {detail.UnpaidLeaveDays} days");
                    attCol.Item().Text($"Overtime Hours: {(detail.OvertimeMinutes / 60.0):0.#} hrs");
                    attCol.Item().Text($"Loss of Pay (LOP): BDT {detail.LossOfPay:N2}").FontColor(detail.LossOfPay > 0 ? Colors.Red.Medium : Colors.Grey.Darken1);
                });
            });

            col.Item().PaddingTop(15);

            // Earnings and Deductions Table
            col.Item().Row(row =>
            {
                var earnings = detail.Components.Where(c => c.ComponentType == "Earning").ToList();
                var deductions = detail.Components.Where(c => c.ComponentType == "Deduction").ToList();

                // Earnings Column Table
                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Column(earnCol =>
                {
                    earnCol.Item().Background(Colors.Indigo.Lighten5).Padding(6).Row(h =>
                    {
                        h.RelativeItem().Text("EARNINGS").Bold().FontColor(Colors.Indigo.Darken2);
                        h.ConstantItem(70).AlignRight().Text("AMOUNT (BDT)").Bold().FontColor(Colors.Indigo.Darken2);
                    });

                    foreach (var e in earnings)
                    {
                        earnCol.Item().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3).Padding(6).Row(r =>
                        {
                            r.RelativeItem().Text(e.ComponentName);
                            r.ConstantItem(70).AlignRight().Text($"{e.Amount:N2}");
                        });
                    }

                    // Total Earnings Row
                    earnCol.Item().Background(Colors.Grey.Lighten4).Padding(6).Row(t =>
                    {
                        t.RelativeItem().Text("GROSS EARNINGS").Bold();
                        t.ConstantItem(70).AlignRight().Text($"{detail.GrossEarnings:N2}").Bold();
                    });
                });

                row.ConstantItem(15);

                // Deductions Column Table
                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Column(dedCol =>
                {
                    dedCol.Item().Background(Colors.Red.Lighten5).Padding(6).Row(h =>
                    {
                        h.RelativeItem().Text("DEDUCTIONS").Bold().FontColor(Colors.Red.Darken2);
                        h.ConstantItem(70).AlignRight().Text("AMOUNT (BDT)").Bold().FontColor(Colors.Red.Darken2);
                    });

                    foreach (var d in deductions)
                    {
                        dedCol.Item().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3).Padding(6).Row(r =>
                        {
                            r.RelativeItem().Text(d.ComponentName);
                            r.ConstantItem(70).AlignRight().Text($"{d.Amount:N2}");
                        });
                    }

                    if (detail.LossOfPay > 0)
                    {
                        dedCol.Item().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3).Padding(6).Row(r =>
                        {
                            r.RelativeItem().Text("Loss of Pay (Absence)");
                            r.ConstantItem(70).AlignRight().Text($"{detail.LossOfPay:N2}").FontColor(Colors.Red.Medium);
                        });
                    }

                    // Total Deductions Row
                    dedCol.Item().Background(Colors.Grey.Lighten4).Padding(6).Row(t =>
                    {
                        t.RelativeItem().Text("TOTAL DEDUCTIONS").Bold();
                        t.ConstantItem(70).AlignRight().Text($"{detail.TotalDeductions:N2}").Bold();
                    });
                });
            });

            col.Item().PaddingTop(15);

            // Net Pay Banner
            col.Item().Background(Colors.Indigo.Darken3).Padding(12).Row(netRow =>
            {
                netRow.RelativeItem().Column(c =>
                {
                    c.Item().Text("NET TAKE-HOME PAY").FontSize(10).SemiBold().FontColor(Colors.White);
                    c.Item().Text(NumberToWords((long)detail.NetPay) + " Taka Only").FontSize(8).Italic().FontColor(Colors.Grey.Lighten2);
                });

                netRow.ConstantItem(150).AlignRight().AlignMiddle()
                    .Text($"BDT {detail.NetPay:N2}").FontSize(16).Bold().FontColor(Colors.Green.Lighten3);
            });

            // Signatures block
            col.Item().PaddingTop(35).Row(sigRow =>
            {
                sigRow.RelativeItem().Column(c =>
                {
                    c.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
                    c.Item().PaddingTop(4).AlignCenter().Text("Employer / HR Authorized Signatory").FontSize(8);
                });

                sigRow.ConstantItem(80);

                sigRow.RelativeItem().Column(c =>
                {
                    c.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
                    c.Item().PaddingTop(4).AlignCenter().Text("Employee Signature / Acknowledgment").FontSize(8);
                });
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(6).Row(row =>
        {
            row.RelativeItem().Text("This is a system-generated payslip from HRMS Enterprise. No physical stamp required.")
                .FontSize(7).FontColor(Colors.Grey.Medium);

            row.ConstantItem(100).AlignRight().Text(text =>
            {
                text.Span("Page ");
                text.CurrentPageNumber();
                text.Span(" of ");
                text.TotalPages();
            });
        });
    }

    private static string NumberToWords(long number)
    {
        if (number == 0) return "Zero";
        if (number < 0) return "Minus " + NumberToWords(Math.Abs(number));

        string words = "";

        if ((number / 10000000) > 0)
        {
            words += NumberToWords(number / 10000000) + " Crore ";
            number %= 10000000;
        }

        if ((number / 100000) > 0)
        {
            words += NumberToWords(number / 100000) + " Lakh ";
            number %= 100000;
        }

        if ((number / 1000) > 0)
        {
            words += NumberToWords(number / 1000) + " Thousand ";
            number %= 1000;
        }

        if ((number / 100) > 0)
        {
            words += NumberToWords(number / 100) + " Hundred ";
            number %= 100;
        }

        if (number > 0)
        {
            var unitsMap = new[] { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
            var tensMap = new[] { "Zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

            if (number < 20)
                words += unitsMap[number];
            else
            {
                words += tensMap[number / 10];
                if ((number % 10) > 0)
                    words += "-" + unitsMap[number % 10];
            }
        }

        return words.Trim();
    }
}
