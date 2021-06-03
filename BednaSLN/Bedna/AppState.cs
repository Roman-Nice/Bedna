namespace Bedna
{
    public class AppState
    {
        public int Change { get; set; }
        public int Credit { get; set; }
        public int Spending { get; set; }
        public int Cost { get; set; }
        public int[] SlotResults;

        public AppState()
        {
            Change = 0;
            Credit = 0;
            Spending = 0;
            Cost = 10;
            SlotResults = new int[3];
        }
    }
}
