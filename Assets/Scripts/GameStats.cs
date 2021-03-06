﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameStats {
    GameController gc;

    public Text resultsText;
    public Text scoreToAddText;
    public Text scoreText;
    public Text spawnInfo;

    public int power { get; private set; }
    public int powerGainedTemp { get; private set; }
    public int spawned { get; private set; }
    public int poped { get; private set; }

    static Vector3 OrigPos;

    float timeToReset;

    public GameStats(GameController gc, Transform canvas, int initialPower) {
        this.gc = gc;
        this.power = initialPower;
        resultsText = canvas.Find("results").GetComponent<Text>();

        scoreToAddText = canvas.Find("scoreToAdd").GetComponent<Text>();
        scoreToAddText.text = "";
        scoreText = canvas.Find("score").GetComponent<Text>();
        scoreText.text = power.ToString();

        spawnInfo = canvas.Find("spawned").GetComponent<Text>();
        spawnInfo.text = "0/0";

        OrigPos = scoreText.transform.position;
    }

    /*public void Summary() {
        CountScore();
        resultsText.text = "Your final score is " + power + " points.";
        if (SetHighscore()) { resultsText.text += "You are getting better! You have beaten your high score"; } else { resultsText.text += "You have " + (PlayerPrefs.GetInt(highScoreKey) - power) + " left to beat your high score"; }
    }*/

    public void SpawnedSphere() {
        spawned++;
        UpdateSphereInfo();
    }

    public void AddPower(int value) {
        powerGainedTemp += value;
        poped++;
        UpdateSphereInfo();

        if (scoreToAddText != null)
            scoreToAddText.text = (powerGainedTemp > 0 ? "+" : "") + powerGainedTemp;

        var temp = timeToReset;
        timeToReset = Time.unscaledTime + 2;
        if (temp < Time.unscaledTime)
            gc.StartCoroutine(ResetWaiter());
    }

    public void AddPowerInstant(int value) {
        power += value;
        scoreText.text = power.ToString();
        if (value != 0)
            gc.StartCoroutine(PowerAnim(value < 0));
    }

    void ResolveImmediateTempPower() {
        AddPowerInstant(powerGainedTemp);
        scoreToAddText.text = "";
        powerGainedTemp = 0;
    }

    void UpdateSphereInfo() {
        spawnInfo.text = poped + "/" + spawned;
    }

    IEnumerator ResetWaiter() {
        while (timeToReset > Time.unscaledTime)
            yield return new WaitForSecondsRealtime(timeToReset - Time.unscaledTime + 0.1f);
        ResolveImmediateTempPower();
    }

    IEnumerator PowerAnim(bool remove = true) {
        if (remove) {
            yield return AnimDown();
            yield return AnimUp();
        } else {
            yield return AnimUp();
            yield return AnimDown();
        }
        scoreText.transform.position = OrigPos;
    }

    IEnumerator AnimUp() {
        Vector3 origPos = scoreText.transform.position + Vector3.up * 2;
        while (scoreText.transform.position.y < origPos.y + 2) {
            scoreText.transform.position += new Vector3(0, Time.deltaTime * 30, 0);
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator AnimDown() {
        Vector3 origPos = scoreText.transform.position + Vector3.down * 2;
        while (scoreText.transform.position.y > origPos.y) {
            scoreText.transform.position -= new Vector3(0, Time.deltaTime * 30, 0);
            yield return new WaitForEndOfFrame();
        }
    }
}
