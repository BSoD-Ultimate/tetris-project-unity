using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TetrisEngine;

namespace GamePlayMode
{
    public abstract class GameMode
    {
        public abstract IEnumerator HandleGameMode();
        public abstract void OnFrameUpdated();
        public abstract void OnGameStart();
        public abstract void OnPieceSpawned();
        public abstract void OnPieceFixed();
        public abstract void OnLineClear(LineClearEvent clearEvent);
        public abstract void OnGameOver();
    }
}
