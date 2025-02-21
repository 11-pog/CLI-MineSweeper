internal class General : SetupCore
{
    protected readonly MineSweeper Field;
    public General(MineSweeper Field) : base(Field)
    {
        this.Field = Field;
    }

    public void Randomize(byte chance)
    {
      float fChance = 1 / chance;

      Field.IterateAllCells((y, x) =>
      {
        RandomizeCell((y, x), fChance);
      });
    }
}