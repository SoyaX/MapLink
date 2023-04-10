using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using Dalamud.Logging;
using Lumina.Excel.GeneratedSheets;
using Newtonsoft.Json;

namespace MapLink; 

public static class Api {

    private static string updateKey = string.Empty;

    private const string ApiBaseUrl = "https://maplink.soyax.app";
    
    public static Stopwatch LastReported { get; } = Stopwatch.StartNew();
    public static bool RequestUpdate { get; set; } = true;
    public static (uint, uint)? LastReportedMap { get; private set; }
    public static Dictionary<string, TreasureSpot?> PartyMaps { get; } = new();
    
    
    public static void UpdateOwnMap() {
        if (ClientState.LocalContentId == 0) return;
        Logic.TryGetCurrentTreasureSpot(out var map);
        
        if (string.IsNullOrEmpty(updateKey)) {
            if (map != null) {
                Put(ClientState.LocalContentId, map?.RowId ?? 0, map?.SubRowId ?? 0);
            }
        } else {
            if (map == null) {
                Delete(ClientState.LocalContentId, updateKey);
            } else {
                Patch(ClientState.LocalContentId, updateKey, map?.RowId ?? 0, map?.SubRowId ?? 0);
            }
        }
    }

    public static void UpdateParty() {
        if (ClientState.LocalContentId == 0) return;
        var partyMemberIds = PartyList.Where(p => p.ContentId != (long)ClientState.LocalContentId).Select(p => (ulong)p.ContentId).ToArray();
        if (partyMemberIds.Length < 1) {
            PartyMaps.Clear();
            return;
        }
        Get(partyMemberIds);
    }

    public static void Delete() {
        if (ClientState.LocalContentId == 0) return;
        if (string.IsNullOrEmpty(updateKey)) return;
        LastReportedMap = null;
        Delete(ClientState.LocalContentId, updateKey);
        PartyMaps.Clear();
    }

    private static async void Put(ulong contentId, uint row, uint subRow) {
        if (contentId == 0) return;
        try {
            using var client = new HttpClient() { Timeout = TimeSpan.FromSeconds(5) };
            LastReportedMap = (row, subRow);
            var response = await client.PutAsync($"{ApiBaseUrl}/{GetHashedContentId(contentId)}/{row}/{subRow}", null);
            if (response.StatusCode == HttpStatusCode.OK) {
                var responseText = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<ApiResponse.PutResponse>(responseText);
                if (responseObject != null) {
                    updateKey = responseObject.Key;
                }
            } else {
                LastReportedMap = null;
            }
        } catch (Exception ex) {
            PluginLog.Error(ex, "Exception in API.Put");
        }
        
    }

    private static async void Patch(ulong contentId, string key, uint row, uint subRow) {
        if (contentId == 0) return;
        try {
            using var client = new HttpClient() { Timeout = TimeSpan.FromSeconds(5) };
            LastReportedMap = (row, subRow);
            var response = await client.PatchAsync($"{ApiBaseUrl}/{GetHashedContentId(contentId)}/{key}/{row}/{subRow}", null);
            if (!response.IsSuccessStatusCode) {
                updateKey = string.Empty;
                RequestUpdate = true;
                LastReportedMap = null;
            }
        } catch (Exception ex) {
            PluginLog.Error(ex, "Exception in API.Patch");
        }
    }

    private static async void Get(ulong[] contentIds) {
        if (contentIds.Length is < 1 or > 7) return;
        try {
            using var client = new HttpClient() { Timeout = TimeSpan.FromSeconds(5) };
            var idList = string.Join(',', contentIds.Select(GetHashedContentId));
            PluginLog.Log($"{ApiBaseUrl}/{idList}");
            var response = await client.GetAsync($"{ApiBaseUrl}/{idList}");
            PartyMaps.Clear();
            if (response.StatusCode == HttpStatusCode.OK) {

                var responseStr = await response.Content.ReadAsStringAsync();
                var responseArray = JsonConvert.DeserializeObject<ApiResponse.GetResponse[]>(responseStr);
                if (responseArray != null) {
                    foreach (var r in responseArray) {
                        PartyMaps.TryAdd(r.ID, r.Row == 0 ? null : DataManager.GetExcelSheet<TreasureSpot>()?.GetRow(r.Row, r.SubRow));
                    }
                }
            } else {
                PluginLog.Error($"Failed: {response.StatusCode}");
            }
        } catch (Exception ex) {
            PluginLog.Error(ex, "Exception in API.Get");
        }
        
    }

    private static async void Delete(ulong contentId, string key) {
        if (contentId == 0) return;
        try {
            using var client = new HttpClient() { Timeout = TimeSpan.FromSeconds(5) };
            LastReportedMap = null;
            await client.DeleteAsync($"{ApiBaseUrl}/{GetHashedContentId(contentId)}/{key}");
            updateKey = string.Empty;
        } catch (Exception ex) {
            PluginLog.Error(ex, "Exception in API.Delete");
        }
        
    }
    
    private static readonly Dictionary<ulong, string> hashedContentIds = new();


    internal static string GetHashedContentId(long contentId) => GetHashedContentId((ulong)contentId);
    internal static string GetHashedContentId(ulong contentId) {
        if (contentId == 0) return string.Empty;
        if (hashedContentIds.TryGetValue(contentId, out var hash)) return hash;
        var md5 = MD5.HashData(Encoding.UTF8.GetBytes($"MapLink__{contentId:X16}"));
        var str = BitConverter.ToString(md5.ToArray()).Replace("-", "");
        hashedContentIds.Add(contentId, str);
        return str;
    }

    public static bool TryGetMapSpotByContentId(long characterContentId, out TreasureSpot? spot) {
        if (characterContentId == (long)ClientState.LocalContentId) return Logic.TryGetCurrentTreasureSpot(out spot);
        return PartyMaps.TryGetValue(GetHashedContentId(characterContentId), out spot);
    }
}
