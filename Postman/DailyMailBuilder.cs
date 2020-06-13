using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Postman
{
    public class DailyMailBuilder
    {
        private readonly string templates;

        public DailyMailBuilder(string templates)
        {
            this.templates = templates;
            ChartDatas = new List<ChartData>();
        }

        public Subscriber Receiver { get; set; }
        public List<ChartData> ChartDatas { get; }

        public string Build()
        {
            string contents = templates;
            contents = contents.Replace("%email%", Receiver.Email);
            contents = contents.Replace("%token%", Receiver.Token);

            StringBuilder chartElementBuilder = new StringBuilder();
            StringBuilder callbackBuilder = new StringBuilder();
            StringBuilder functionBuilder = new StringBuilder();
            for (int i = 0; i < ChartDatas.Count; i++)
            {
                chartElementBuilder.Append($"<tr><div id = \"chart{i}\" style = \"border: 1px solid #ccc;\"></div></tr> ");
                callbackBuilder.Append($"google.charts.setOnLoadCallback(drawChart{i}); ");
                functionBuilder.Append(ChartDatas[i].BuildFunction($"drawChart{i}", $"chart{i}"));
            }
            contents = contents.Replace("%chart%", chartElementBuilder.ToString());
            contents = contents.Replace("%callback%", callbackBuilder.ToString());
            contents = contents.Replace("%function%", functionBuilder.ToString());

            return contents;
        }

        public class ChartData
        {
            public ChartData(string title, Dictionary<DateTime, int> closingPrices, Dictionary<DateTime, int> predictPrices)
            {
                Title = title ?? throw new ArgumentNullException(nameof(title));
                ClosingPrices = closingPrices ?? throw new ArgumentNullException(nameof(closingPrices));
                PredictPrices = predictPrices ?? throw new ArgumentNullException(nameof(predictPrices));
            }

            public string Title { get; }
            public Dictionary<DateTime, int> ClosingPrices { get; }
            public Dictionary<DateTime, int> PredictPrices { get; }

            public string BuildFunction(string functionName, string chartElementId)
            {
                StringBuilder builder = new StringBuilder();

                builder.Append("function ");
                builder.Append(functionName);
                builder.Append("() { var data = new google.visualization.DataTable(); data.addColumn('date', 'X'); data.addColumn('number', '종가'); data.addColumn('number', '예측가'); data.addRows(");

                builder.Append("[ ");
                IEnumerable<DateTime> dates = ClosingPrices.Keys.Union(PredictPrices.Keys).OrderBy(x => x);
                foreach (DateTime date in dates)
                {
                    builder.Append("[new Date(");
                    builder.Append(date.Year);
                    builder.Append(", ");
                    builder.Append(date.Month - 1);
                    builder.Append(", ");
                    builder.Append(date.Day);
                    builder.Append("), ");
                    if (ClosingPrices.TryGetValue(date, out int price))
                    {
                        builder.Append(price);
                    }
                    else
                    {
                        builder.Append("null");
                    }
                    builder.Append(", ");
                    if (PredictPrices.TryGetValue(date, out price))
                    {
                        builder.Append(price);
                    }
                    else
                    {
                        builder.Append("null");
                    }
                    builder.Append("], ");
                }
                builder.Append(" ]");
                
                builder.Append("); var options = { title: '");
                builder.Append(Title);
                builder.Append("', height: 300, hAxis: { title: '날짜', format: 'yy/M/d', gridlines: { count: 15 } }, vAxis: { title: '종가 (원)' } }; var chart = new google.visualization.LineChart(document.getElementById('");
                builder.Append(chartElementId);
                builder.Append("')); chart.draw(data, options); } ");

                return builder.ToString();
            }
        }
    }
}
