using TetrisTactic.PlayField;

namespace TetrisTactic.PlayerTurn
{
    public sealed class MoveHighlighter
    {
        private readonly PlayFieldController playFieldController;

        public MoveHighlighter(PlayFieldController playFieldController)
        {
            this.playFieldController = playFieldController;
        }

        public void HighlightLegalMoves()
        {
            var legalMoves = playFieldController.GetLegalPlayerMoveCells();
            playFieldController.SetMoveHighlights(legalMoves);
        }

        public void Clear()
        {
            playFieldController.ClearMoveHighlights();
        }
    }
}
