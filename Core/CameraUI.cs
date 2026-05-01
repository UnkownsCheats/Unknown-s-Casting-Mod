using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DevCameraMod.Models
{ 
    [Serializable]
    public class CameraUI
    { 
        public void AdjustTeam(bool add, bool team)
        {
            if (team)
            {
                int num = int.Parse(this.leftPoints.text);
                if (add)
                {
                    num++;
                }
                else
                {
                    num--;
                }
                this.leftPoints.text = num.ToString();
            }
            else
            {
                int num2 = int.Parse(this.rightPoints.text);
                if (add)
                {
                    num2++;
                }
                else
                {
                    num2--;
                }
                this.rightPoints.text = num2.ToString();
            }
        }
  
        public Canvas canvas; 
        public TMP_Text cameraSpectator; 
        public TMP_Text currentlySpectating; 
        public RawImage currentSpecImage; 
        public TMP_Text leftTeam; 
        public TMP_Text rightTeam; 
        public TMP_Text leftPoints; 
        public TMP_Text rightPoints; 
        public TMP_Text currentTime; 
        public TMP_Text lapTime; 
        public TMP_Text scoreboardText; 
        public TMP_Text scoreboardText2; 
        public TMP_Text versionTex; 
        public TMP_Text version2;
        public TMP_Text codeSecret; 
        public TMP_Text scoreHeader; 
        public double timeStamp; 
        public Transform Sponsors; 
        public string LeftTeamName = "left"; 
        public string RightTeamName = "right";
    }
}