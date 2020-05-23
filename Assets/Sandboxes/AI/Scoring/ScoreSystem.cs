using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace GameSystem
{
    public class ScoreSystem : MonoBehaviour
    {
        /*************************************************/

        #region public vars
        public enum Themes
        {
            environment,
            technology
        }
        public enum Games
        {
            firstGame,
            secondGame,
            thirdGame
        }

        public int GameCount { get { return scoreBoard.gameCount; } }
        public float MaxValue { get { return scoreBoard.currentMax; } }
        #endregion



        /*************************************************/

        #region private vars

        [SerializeField] private bool nuitBlanche = false;
        [SerializeField] private float nuitBlancheMult = 10.0f;
        private float globalScoreMult = 1;
        private static int totalGames = 3;

        private class ScoreBoard
        {
            public int gameCount = 0;
            public List<Vector2>[][] board = new List<Vector2>[2][];
            public float currentMax = 0;

            public ScoreBoard(int _totalGames)
            {
                for( int i=0; i<board.Length;  i++)
                {
                    board[i] = new List<Vector2>[_totalGames];
                }
            }
        }
        private ScoreBoard scoreBoard = new ScoreBoard(totalGames);

        private string path = Path.Combine(Application.persistentDataPath, "scoreBoard.json");
        #endregion

        /*************************************************/


        /*********************************************************************************************************/
        #region setups
        ////////////////////////////////////////////////////////////////////////////////////////////
        private void Awake()
        {
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                JsonUtility.FromJsonOverwrite(json, scoreBoard);
            }
            else
            {
                SaveJSONScore();
            }
            if(nuitBlanche)
            {
                globalScoreMult *= nuitBlancheMult;
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////
        #endregion
        /*********************************************************************************************************/


        /*********************************************************************************************************/
        #region public functions
        ////////////////////////////////////////////////////////////////////////////////////////////

        public void AppendScores(float[] _scores, Themes _theme, Games _game)
        {
            scoreBoard.board[(int)_theme][(int)_game].Add(new Vector2(_scores[0], _scores[1]));
            foreach (float f in _scores)
                if (f > scoreBoard.currentMax) scoreBoard.currentMax = f;
            scoreBoard.gameCount++;
            SaveJSONScore();
        }
        public float[] GetSpecificScores(Themes _theme, Games _game, int idx)
        {

            float[] scores = { scoreBoard.board[(int)_theme][(int)_game][idx].x, scoreBoard.board[(int)_theme][(int)_game][idx].y };
            return scores;
        }
        public float[] GetLatestScores(Themes _theme, Games _game)
        {
            int currentIdx = scoreBoard.gameCount - 1;
            float[] scores = { scoreBoard.board[(int)_theme][(int)_game][currentIdx].x, scoreBoard.board[(int)_theme][(int)_game][currentIdx].y };
            return scores;
        }
        public float[][] GetLearningCurves(Themes _theme)
        {
            float[][] curves = new float[totalGames][];

            for(int i=0; i<curves.Length; i++)
            {
                Vector2[] vecBuffer = scoreBoard.board[(int)_theme][i].ToArray();
                float[] vecSums = new float[vecBuffer.Length];
                for(int j=0; j< vecBuffer.Length; j++)
                {
                    vecSums[j] = vecBuffer[j].x - vecBuffer[j].y;
                    if (Mathf.Abs(vecSums[j]) >= scoreBoard.currentMax) scoreBoard.currentMax = Mathf.Abs(vecSums[j]);
                }
                curves[i] = vecSums;
            }

            return curves;
        }
        public float[] GetGamesLearningCurve(Themes _theme, Games _game)
        {
            Vector2[] vecBuffer = scoreBoard.board[(int)_theme][(int)_game].ToArray();
            float[] vecSums = new float[vecBuffer.Length];
            for (int j = 0; j < vecBuffer.Length; j++)
            {
                vecSums[j] = vecBuffer[j].x - vecBuffer[j].y;
                if (Mathf.Abs(vecSums[j]) >= scoreBoard.currentMax) scoreBoard.currentMax = Mathf.Abs(vecSums[j]);
            }
            return vecSums;
        }
        public float GetSpecicLearningCurve(Themes _theme, Games _game, int idx)
        {
            
            Vector2[] vecBuffer = scoreBoard.board[(int)_theme][(int)_game].ToArray();
            float specificKnowledge= vecBuffer[idx].x - vecBuffer[idx].y;
            return specificKnowledge;

        }
        public float[] GetLatestLearningCurves(Themes _theme)
        {
            float[] sums = new float[totalGames];
            for (int i = 0; i < sums.Length; i++)
            {
                Vector2[] vecBuffer = scoreBoard.board[(int)_theme][i].ToArray();
                sums[i] = vecBuffer[scoreBoard.gameCount - 1].x - vecBuffer[scoreBoard.gameCount - 1].y;
            }
            return sums;

        }
        ////////////////////////////////////////////////////////////////////////////////////////////
        #endregion
        /*********************************************************************************************************/


        /*********************************************************************************************************/
        #region file system
        ////////////////////////////////////////////////////////////////////////////////////////////
        private void SaveJSONScore()
        {
            string json = JsonUtility.ToJson(scoreBoard);
            File.WriteAllText(path, json);
        }
        ////////////////////////////////////////////////////////////////////////////////////////////
        #endregion
        /*********************************************************************************************************/

    }

}
