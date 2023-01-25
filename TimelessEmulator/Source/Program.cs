using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using Spectre.Console;
using TimelessEmulator.Data;
using TimelessEmulator.Data.Models;
using TimelessEmulator.Game;

namespace TimelessEmulator;

public static class Program
{

    static Program()
    {

    }

    public static void Main(string[] arguments)
    {
        Console.Title = $"{Settings.ApplicationName} Ver. {Settings.ApplicationVersion}";
        if (!DataManager.Initialize())
            ExitWithError("Failed to initialize th   e [yellow]data manager[/].");

        for (uint jewel_version = 1; jewel_version < 6; jewel_version++){
            string jewel_name = "";
            uint min_seed = 0;
            uint max_seed = 0;
            switch (jewel_version)
            {
                case 1:
                    jewel_name = "glorious_vanity";
                    min_seed = 100;
                    max_seed = 8000;
                    break;
                case 2:
                    jewel_name = "lethal_pride";
                    min_seed = 10000;
                    max_seed = 18000;
                    break;
                case 3:
                    jewel_name = "brutal_restraint";
                    min_seed = 500;
                    max_seed = 8000;
                    break;
                case 4:
                    jewel_name = "militant_faith";
                    min_seed = 2000;
                    max_seed = 10000;
                    break;
                case 5:
                    jewel_name = "elegant_hubris";
                    min_seed = 100;
                    max_seed = 8000;
                    break;
            }
            List<PassiveSkill> passive_skills = DataManager.getPassiveSkills(jewel_name);
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TimelessJewels", jewel_name);
            System.IO.Directory.CreateDirectory(path);

            for (uint seed=min_seed; seed<max_seed+1; seed++){
                if (seed % 100 == 0){
                    Console.WriteLine(jewel_name + ": Seed " + seed.ToString() + " out of " + max_seed.ToString());
                }
                uint alt_seed = seed;
                if (jewel_version == 5){
                    alt_seed *= 20;
                }
                TimelessJewel timelessJewel = FetchTimelessJewel(alt_seed, jewel_version);

                using (StreamWriter outputFile = new StreamWriter(Path.Combine(path, alt_seed.ToString() + ".csv"))){

                    foreach (PassiveSkill passiveSkill in passive_skills) {
                        string output = "";
                        AlternateTreeManager alternateTreeManager = new AlternateTreeManager(passiveSkill, timelessJewel);
                        if (alternateTreeManager.IsPassiveSkillReplaced())
                        {
                            output = "1";
                            AlternatePassiveSkillInformation alternatePassiveSkillInformation = alternateTreeManager.ReplacePassiveSkill();
                            for (int i = 0; i < alternatePassiveSkillInformation.AlternatePassiveSkill.StatIndices.Count; i++)
                            {
                                uint statIndex = alternatePassiveSkillInformation.AlternatePassiveSkill.StatIndices.ElementAt(i);
                                uint statRoll = alternatePassiveSkillInformation.StatRolls.ElementAt(i).Value;
                                output += "," + statIndex.ToString() + "," + statRoll.ToString();
                            }
                            foreach (AlternatePassiveAdditionInformation alternatePassiveAdditionInformation in alternatePassiveSkillInformation.AlternatePassiveAdditionInformations)
                                {
                                    for (int i = 0; i < alternatePassiveAdditionInformation.AlternatePassiveAddition.StatIndices.Count; i++)
                                    {
                                        uint statIndex = alternatePassiveAdditionInformation.AlternatePassiveAddition.StatIndices.ElementAt(i);
                                        uint statRoll = alternatePassiveAdditionInformation.StatRolls.ElementAt(i).Value;
                                        output += "," + statIndex.ToString() + "," + statRoll.ToString();
                                    }
                                }
                        } else {
                            output = "0";
                            foreach (AlternatePassiveAdditionInformation alternatePassiveAdditionInformation in alternateTreeManager.AugmentPassiveSkill())
                                {
                                    for (int i = 0; i < alternatePassiveAdditionInformation.AlternatePassiveAddition.StatIndices.Count; i++)
                                    {
                                        uint statIndex = alternatePassiveAdditionInformation.AlternatePassiveAddition.StatIndices.ElementAt(i);
                                        uint statRoll = alternatePassiveAdditionInformation.StatRolls.ElementAt(i).Value;
                                        output += "," + statIndex.ToString() + "," + statRoll.ToString();
                                    }
                                }
                        }
                        outputFile.WriteLine(output);
                    }
                }
            }
        }
        WaitForExit();
    }

    private static PassiveSkill GetPassiveSkillFromInput()
    {
        TextPrompt<string> passiveSkillTextPrompt = new TextPrompt<string>("[green]Passive Skill[/]:")
            .Validate((string input) =>
            {
                PassiveSkill passiveSkill = DataManager.GetPassiveSkillByFuzzyValue(input);

                if (passiveSkill == null)
                    return ValidationResult.Error($"[red]Error[/]: Unable to find [yellow]passive skill[/] `{input}`.");

                if (!DataManager.IsPassiveSkillValidForAlteration(passiveSkill))
                    return ValidationResult.Error($"[red]Error[/]: The [yellow]passive skill[/] `{input}` is not valid for alteration.");

                return ValidationResult.Success();
            });

        string passiveSkillInput = AnsiConsole.Prompt(passiveSkillTextPrompt);

        PassiveSkill passiveSkill = DataManager.GetPassiveSkillByFuzzyValue(passiveSkillInput);

        AnsiConsole.MarkupLine($"[green]Found Passive Skill[/]: [yellow]{passiveSkill.Name}[/] ([yellow]{passiveSkill.Identifier}[/])");

        return passiveSkill;
    }


    private static TimelessJewel FetchTimelessJewel(uint seed, uint jewel_type){
        AlternateTreeVersion alternateTreeVersion = DataManager.AlternateTreeVersions.First(q => (q.Index == jewel_type));
        TimelessJewelConqueror timelessJewelConqueror = new TimelessJewelConqueror(1, 0);
        return new TimelessJewel(alternateTreeVersion, timelessJewelConqueror, seed);
    }
    
    private static TimelessJewel GetTimelessJewelFromInput()
    {
        Dictionary<uint, string> timelessJewelTypes = new Dictionary<uint, string>()
        {
            { 1, "Glorious Vanity" },
            { 2, "Lethal Pride" },
            { 3, "Brutal Restraint" },
            { 4, "Militant Faith" },
            { 5, "Elegant Hubris" }
        };

        Dictionary<uint, Dictionary<string, TimelessJewelConqueror>> timelessJewelConquerors = new Dictionary<uint, Dictionary<string, TimelessJewelConqueror>>()
        {
            {
                1, new Dictionary<string, TimelessJewelConqueror>()
                {
                    { "Xibaqua", new TimelessJewelConqueror(1, 0) },
                        { "[springgreen3]Zerphi (Legacy)[/]", new TimelessJewelConqueror(2, 0) },
                    { "Ahuana", new TimelessJewelConqueror(2, 1) },
                    { "Doryani", new TimelessJewelConqueror(3, 0) }
                }
            },
            {
                2, new Dictionary<string, TimelessJewelConqueror>()
                {
                    { "Kaom", new TimelessJewelConqueror(1, 0) },
                    { "Rakiata", new TimelessJewelConqueror(2, 0) },
                    { "[springgreen3]Kiloava (Legacy)[/]", new TimelessJewelConqueror(3, 0) },
                    { "Akoya", new TimelessJewelConqueror(3, 1) }
                }
            },
            {
                3, new Dictionary<string, TimelessJewelConqueror>()
                {
                    { "[springgreen3]Deshret (Legacy)[/]", new TimelessJewelConqueror(1, 0) },
                    { "Balbala", new TimelessJewelConqueror(1, 1) },
                    { "Asenath", new TimelessJewelConqueror(2, 0) },
                    { "Nasima", new TimelessJewelConqueror(3, 0) }
                }
            },
            {
                4, new Dictionary<string, TimelessJewelConqueror>()
                {
                    { "[springgreen3]Venarius (Legacy)[/]", new TimelessJewelConqueror(1, 0) },
                    { "Maxarius", new TimelessJewelConqueror(1, 1) },
                    { "Dominus", new TimelessJewelConqueror(2, 0) },
                    { "Avarius", new TimelessJewelConqueror(3, 0) }
                }
            },
            {
                5, new Dictionary<string, TimelessJewelConqueror>()
                {
                    { "Cadiro", new TimelessJewelConqueror(1, 0) },
                    { "Victario", new TimelessJewelConqueror(2, 0) },
                    { "[springgreen3]Chitus (Legacy)[/]", new TimelessJewelConqueror(3, 0) },
                    { "Caspiro", new TimelessJewelConqueror(3, 1) }
                }
            }
        };

        Dictionary<uint, (uint minimumSeed, uint maximumSeed)> timelessJewelSeedRanges = new Dictionary<uint, (uint minimumSeed, uint maximumSeed)>()
        {
            { 1, (100, 8000) },
            { 2, (10000, 18000) },
            { 3, (500, 8000) },
            { 4, (2000, 10000) },
            { 5, (2000, 160000) }
        };

        SelectionPrompt<string> timelessJewelTypeSelectionPrompt = new SelectionPrompt<string>()
            .Title("[green]Timeless Jewel Type[/]:")
            .AddChoices(timelessJewelTypes.Values.ToArray());

        string timelessJewelTypeInput = AnsiConsole.Prompt(timelessJewelTypeSelectionPrompt);

        AnsiConsole.MarkupLine($"[green]Timeless Jewel Type[/]: {timelessJewelTypeInput}");

        uint alternateTreeVersionIndex = timelessJewelTypes
            .First(q => (q.Value == timelessJewelTypeInput))
            .Key;

        AlternateTreeVersion alternateTreeVersion = DataManager.AlternateTreeVersions
            .First(q => (q.Index == alternateTreeVersionIndex));

        SelectionPrompt<string> timelessJewelConquerorSelectionPrompt = new SelectionPrompt<string>()
            .Title("[green] Timeless Jewel Conqueror[/]:")
            .AddChoices(timelessJewelConquerors[alternateTreeVersionIndex].Keys.ToArray());

        string timelessJewelConquerorInput = AnsiConsole.Prompt(timelessJewelConquerorSelectionPrompt);

        AnsiConsole.MarkupLine($"[green]Timeless Jewel Conqueror[/]: {timelessJewelConquerorInput}");

        TimelessJewelConqueror timelessJewelConqueror = timelessJewelConquerors[alternateTreeVersionIndex]
            .First(q => (q.Key == timelessJewelConquerorInput))
            .Value;

        TextPrompt<uint> timelessJewelSeedTextPrompt = new TextPrompt<uint>($"[green]Timeless Jewel Seed ({timelessJewelSeedRanges[alternateTreeVersionIndex].minimumSeed} - {timelessJewelSeedRanges[alternateTreeVersionIndex].maximumSeed})[/]:")
            .Validate((uint input) =>
            {
                if ((input >= timelessJewelSeedRanges[alternateTreeVersionIndex].minimumSeed) &&
                    (input <= timelessJewelSeedRanges[alternateTreeVersionIndex].maximumSeed))
                {
                    return ValidationResult.Success();
                }

                return ValidationResult.Error($"[red]Error[/]: The [yellow]timeless jewel seed[/] must be between {timelessJewelSeedRanges[alternateTreeVersionIndex].minimumSeed} and {timelessJewelSeedRanges[alternateTreeVersionIndex].maximumSeed}.");
            });

        uint timelessJewelSeed = AnsiConsole.Prompt(timelessJewelSeedTextPrompt);

        return new TimelessJewel(alternateTreeVersion, timelessJewelConqueror, timelessJewelSeed);
    }

    private static void PrintAlternatePassiveAdditionInformations(IReadOnlyCollection<AlternatePassiveAdditionInformation> alternatePassiveAdditionInformations)
    {
        ArgumentNullException.ThrowIfNull(alternatePassiveAdditionInformations, nameof(alternatePassiveAdditionInformations));

        foreach (AlternatePassiveAdditionInformation alternatePassiveAdditionInformation in alternatePassiveAdditionInformations)
        {
            AnsiConsole.MarkupLine($"\t[green]Addition[/]: [yellow]{alternatePassiveAdditionInformation.AlternatePassiveAddition.Identifier}[/]");

            for (int i = 0; i < alternatePassiveAdditionInformation.AlternatePassiveAddition.StatIndices.Count; i++)
            {
                uint statIndex = alternatePassiveAdditionInformation.AlternatePassiveAddition.StatIndices.ElementAt(i);
                uint statRoll = alternatePassiveAdditionInformation.StatRolls.ElementAt(i).Value;

                AnsiConsole.MarkupLine($"\t\tStat [yellow]{i}[/] | [yellow]{DataManager.GetStatTextByIndex(statIndex)}[/] (Identifier: [yellow]{DataManager.GetStatIdentifierByIndex(statIndex)}[/], Index: [yellow]{statIndex}[/]), Roll: [yellow]{statRoll}[/]");
            }
        }
    }

    private static void WaitForExit()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("Press [yellow]any key[/] to exit.");

        try
        {
            Console.ReadKey();
        }
        catch { }

        Environment.Exit(0);
    }

    private static void PrintError(string error)
    {
        AnsiConsole.MarkupLine($"[red]Error[/]: {error}");
    }

    private static void ExitWithError(string error)
    {
        PrintError(error);
        WaitForExit();
    }
}
