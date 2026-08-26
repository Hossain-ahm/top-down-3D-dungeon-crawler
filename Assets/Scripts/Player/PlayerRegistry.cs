using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Tracks all active players in the scene.
/// Enemies query this to find their nearest target rather than
/// holding a single hardcoded player reference.
/// </summary>
public static class PlayerRegistry
{
    private static readonly List<Transform> _players = new List<Transform>();

    public static IReadOnlyList<Transform> Players => _players;

    public static void Register(Transform player)
    {
        if (!_players.Contains(player))
            _players.Add(player);
    }

    public static void Unregister(Transform player)
    {
        _players.Remove(player);
    }

    public static Transform GetNearest(Vector3 fromPosition)
    {
        Transform nearest = null;
        float nearestSqr = float.MaxValue;

        for (int i = 0; i < _players.Count; i++)
        {
            if (_players[i] == null) continue;

            float sqrDist = (_players[i].position - fromPosition).sqrMagnitude;
            if (sqrDist < nearestSqr)
            {
                nearestSqr = sqrDist;
                nearest = _players[i];
            }
        }

        return nearest;
    }
}