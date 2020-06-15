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

            contents = contents.Replace("%predictdate%", DateTime.Today.ToString("yyyy년 M월 d일"));

            StringBuilder chartElementBuilder = new StringBuilder();
            foreach (ChartData chartData in ChartDatas.Distinct())
            {
                chartElementBuilder.Append("<tr>");
                chartElementBuilder.Append($"<img src=\"https://s3.ap-northeast-2.amazonaws.com/prediction.bucket/PredictPNG/Predict_{chartData.StockId}.png\" alt=\"chart\" style=\"height: 500px; width: auto; margin-top: 20px; border: 1px solid #ccc;\">");
                chartElementBuilder.AppendLine($"<p>오늘 종가: {chartData.TodayClosingPrice}원</p>");
                chartElementBuilder.AppendLine($"<p>앞으로 {chartData.PredictClosingPrices.Count()}일 간 예측가: {string.Join(",", chartData.PredictClosingPrices)}원</p>");
                chartElementBuilder.Append("</tr>");
            }
            contents = contents.Replace("%chart%", chartElementBuilder.ToString());

            return contents;
        }

        public class ChartData : IEquatable<ChartData>
        {
            public ChartData(string stockId, int todayClosingPrice, IEnumerable<int> predictClosingPrices)
            {
                StockId = stockId ?? throw new ArgumentNullException(nameof(stockId));
                TodayClosingPrice = todayClosingPrice;
                PredictClosingPrices = predictClosingPrices ?? throw new ArgumentNullException(nameof(predictClosingPrices));
            }

            public string StockId { get; }
            public int TodayClosingPrice { get; }
            public IEnumerable<int> PredictClosingPrices { get; }

            public override bool Equals(object obj)
            {
                return Equals(obj as ChartData);
            }

            public bool Equals(ChartData other)
            {
                return other != null &&
                       StockId == other.StockId;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(StockId);
            }

            public static bool operator ==(ChartData left, ChartData right)
            {
                return EqualityComparer<ChartData>.Default.Equals(left, right);
            }

            public static bool operator !=(ChartData left, ChartData right)
            {
                return !(left == right);
            }
        }

        //public class ChartData
        //{
        //    public ChartData(string title, Dictionary<DateTime, int> closingPrices, Dictionary<DateTime, int> predictPrices)
        //    {
        //        Title = title ?? throw new ArgumentNullException(nameof(title));
        //        ClosingPrices = closingPrices ?? throw new ArgumentNullException(nameof(closingPrices));
        //        PredictPrices = predictPrices ?? throw new ArgumentNullException(nameof(predictPrices));
        //    }

        //    public string Title { get; }
        //    public Dictionary<DateTime, int> ClosingPrices { get; }
        //    public Dictionary<DateTime, int> PredictPrices { get; }

        //    public string BuildFunction(string functionName, string chartElementId)
        //    {
        //        StringBuilder builder = new StringBuilder();

        //        builder.Append("function ");
        //        builder.Append(functionName);
        //        builder.Append("() { var data = new google.visualization.DataTable(); data.addColumn('date', 'X'); data.addColumn('number', '종가'); data.addColumn('number', '예측가'); data.addRows(");

        //        builder.Append("[ ");
        //        IEnumerable<DateTime> dates = ClosingPrices.Keys.Union(PredictPrices.Keys).OrderBy(x => x);
        //        foreach (DateTime date in dates)
        //        {
        //            builder.Append("[new Date(");
        //            builder.Append(date.Year);
        //            builder.Append(", ");
        //            builder.Append(date.Month - 1);
        //            builder.Append(", ");
        //            builder.Append(date.Day);
        //            builder.Append("), ");
        //            if (ClosingPrices.TryGetValue(date, out int price))
        //            {
        //                builder.Append(price);
        //            }
        //            else
        //            {
        //                builder.Append("null");
        //            }
        //            builder.Append(", ");
        //            if (PredictPrices.TryGetValue(date, out price))
        //            {
        //                builder.Append(price);
        //            }
        //            else
        //            {
        //                builder.Append("null");
        //            }
        //            builder.Append("], ");
        //        }
        //        builder.Append(" ]");
                
        //        builder.Append("); var options = { title: '");
        //        builder.Append(Title);
        //        builder.Append("', height: 300, hAxis: { title: '날짜', format: 'yy/M/d', gridlines: { count: 15 } }, vAxis: { title: '종가 (원)' } }; var chart = new google.visualization.LineChart(document.getElementById('");
        //        builder.Append(chartElementId);
        //        builder.Append("')); chart.draw(data, options); } ");

        //        return builder.ToString();
        //    }
        //}
    }
}
