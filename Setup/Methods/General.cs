namespace CLI_MineSweeper
{
    public class General : SetupCore
    {
        protected readonly MineSweeper Field;
        internal General(MineSweeper Field) : base(Field)
        {
            this.Field = Field;
        }

        internal void Randomize(byte chance)
        {
            float fChance = 1 / chance;

            Field.IterateAllCells((coords) =>
            {
                RandomizeCell(coords, fChance);
            });
        }
    }
}