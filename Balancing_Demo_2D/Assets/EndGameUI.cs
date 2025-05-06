using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using TMPro;
using UnityEngine.UI;
using Unity.Collections;
using System;
using System.Collections;

public class EndGameUI : NetworkBehaviour
{
    private static GameManager GM;

    [SerializeField] private GameObject IGUI;
    [SerializeField] private GameObject augUI;
    [SerializeField] private GameObject endGameUI;
    [SerializeField] private GameObject player1Stats;
    [SerializeField] private GameObject player2Stats;

    [SerializeField] private List<GameObject> player1StatsList;
    [SerializeField] private List<GameObject> player2StatsList;

    public NetworkVariable<bool> statsAssigned = new NetworkVariable<bool>(false);

    public NetworkList<StatBlock> p1Stats = new NetworkList<StatBlock>();
    public NetworkList<StatBlock> p2Stats = new NetworkList<StatBlock>();

    private void Awake()
    {
        p1Stats.OnListChanged += OnStatsChanged;
        p2Stats.OnListChanged += OnStatsChanged;
        statsAssigned.OnValueChanged += OnStatsAssignedChanged;
    }

    void Start()
    {
        GM = GameManager.Instance;
        if (GM == null)
        {
            Debug.LogError("GameManager instance is null.");
        }
    }

    public void StatsToList()
    {
        if (!IsServer) return;

        List<string> p1StatsRaw = GM.findStats(GM.player1ID);
        List<string> p2StatsRaw = GM.findStats(GM.player2ID);

        p1Stats.Clear();
        p2Stats.Clear();

        foreach (var stat in p1StatsRaw)
            p1Stats.Add(new StatBlock(stat));
        foreach (var stat in p2StatsRaw)
            p2Stats.Add(new StatBlock(stat));

        statsAssigned.Value = true;  // Triggers OnValueChanged on all clients
    }

    public void DisplayEndGameUI(ulong p1, ulong p2)
    {
        HIDEALLOTHERUI();
        endGameUI.SetActive(true);
        player1Stats.SetActive(false);
        player2Stats.SetActive(false);

        StartCoroutine(DisplayStats());
    }

    private void HIDEALLOTHERUI()
    {
        IGUI.SetActive(false);
        augUI.SetActive(false);
    }

    private void OnStatsChanged(NetworkListEvent<StatBlock> change)
    {
        if (statsAssigned.Value)
        {
            List<StatBlock> p1 = new List<StatBlock>();
            List<StatBlock> p2 = new List<StatBlock>();

            foreach (var s in p1Stats) p1.Add(s);
            foreach (var s in p2Stats) p2.Add(s);

            UpdateStatsUI(p1, p2);
        }
    }

    private void OnStatsAssignedChanged(bool previousValue, bool newValue)
    {
        if (newValue)
        {
            List<StatBlock> p1 = new List<StatBlock>();
            List<StatBlock> p2 = new List<StatBlock>();

            foreach (var s in p1Stats) p1.Add(s);
            foreach (var s in p2Stats) p2.Add(s);

            UpdateStatsUI(p1, p2);
        }
    }

    public void UpdateStatsUI(IReadOnlyList<StatBlock> stats1, IReadOnlyList<StatBlock> stats2)
    {
        for (int i = 0; i < 7 && i < stats1.Count && i < stats2.Count; i++)
        {
            player1StatsList[i].GetComponent<TextMeshProUGUI>().text = stats1[i].value.ToString();
            player2StatsList[i].GetComponent<TextMeshProUGUI>().text = stats2[i].value.ToString();

            player1StatsList[i].SetActive(false);
            player2StatsList[i].SetActive(false);
        }
    }

    private IEnumerator DisplayStats()
    {
        yield return new WaitUntil(() => statsAssigned.Value);

        player1Stats.SetActive(true);
        player2Stats.SetActive(true);

        for (int i = 0; i < player1StatsList.Count; i++)
        {
            player1StatsList[i].SetActive(true);
            player2StatsList[i].SetActive(true);
            yield return new WaitForSeconds(1f);
        }
    }
}

[System.Serializable]
public struct StatBlock : INetworkSerializable, IEquatable<StatBlock>
{
    public FixedString64Bytes value;

    public StatBlock(string v) => value = v;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref value);
    }

    public bool Equals(StatBlock other)
    {
        return value.Equals(other.value);
    }

    public override bool Equals(object obj)
    {
        return obj is StatBlock other && Equals(other);
    }

    public override int GetHashCode()
    {
        return value.GetHashCode();
    }
}
