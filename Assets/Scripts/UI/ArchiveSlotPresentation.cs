using System;

// Presentation data only; a future save service can populate these fields.
[Serializable]
public sealed class ArchiveSlotPresentation
{
    public bool occupied;
    public string unitName = "";
    public string playTime = "--:--:--";
    public string level = "LEVEL 01 // TORRE DEL OLVIDO";
    public string checkpoint = "NO ANCHOR RECORDED";
    public float progress;
    public string lastRecord = "NO RECORD YET";

    public static ArchiveSlotPresentation Empty() => new();

    public static ArchiveSlotPresentation Example(int index) => new()
    {
        occupied = true,
        unitName = index == 0 ? "ECHO UNIT // ALPHA" : "ECHO UNIT // BETA",
        playTime = index == 0 ? "01:24:35" : "04:52:10",
        level = "LEVEL 01 // TORRE DEL OLVIDO",
        checkpoint = index == 0 ? "ANCHOR // TEMPORAL GATE" : "ANCHOR // UPPER TOWER",
        progress = index == 0 ? .65f : .82f,
        lastRecord = index == 0 ? "2 MINS AGO" : "YESTERDAY",
    };
}
