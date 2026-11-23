using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace Jpt.MoodyJapanese
{
    public sealed class JapaneseMod : Verse.Mod
    {
        public JapaneseMod(Verse.ModContentPack pack) : base(pack) { DisplayTranslation.Install(); }
    }

    public static partial class DisplayTranslation
    {
        public sealed class Rule
        {
            public readonly string Text;
            public readonly int Expected;
            public readonly int[] Occurrences;
            public Rule(string text, int expected, int[] occurrences)
            { Text = text; Expected = expected; Occurrences = occurrences; }
        }
        static readonly Dictionary<string, string> Texts = new Dictionary<string, string>(StringComparer.Ordinal);
        static readonly Dictionary<string, List<Rule>> Rules = new Dictionary<string, List<Rule>>(StringComparer.Ordinal);
        static bool installed;
        static MethodInfo translate;
        static DisplayTranslation() { Populate(); }
        static void AddRule(string target, string text, int expected, int[] occurrences)
        {
            List<Rule> list;
            if (!Rules.TryGetValue(target, out list)) Rules.Add(target, list = new List<Rule>());
            list.Add(new Rule(text, expected, occurrences));
        }
        public static void Install()
        {
            if (installed) return;
            installed = true;
            try
            {
                Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "Moody");
                if (assembly == null) { Warn("Moody assembly not found; localization skipped."); return; }
                Harmony harmony = new Harmony("jpt.giantspacehamster.moody.display");
                HashSet<MethodBase> patched = new HashSet<MethodBase>();
                foreach (string target in Rules.Keys.Concat(new[] {
                    "Moody.Views.ThoughtsView::DrawThoughtGroupTable",
                    "Moody.Table.Columns.ThoughtGroupColumn::CellRenderer" }).Distinct())
                {
                    int split = target.LastIndexOf("::", StringComparison.Ordinal);
                    Type type = assembly.GetType(target.Substring(0, split), false);
                    string name = target.Substring(split + 2);
                    if (type == null) { Warn("Display type not found: " + target); continue; }
                    // TemperatureBandInfo tooltips are localized by their getter, even if cached early.
                    if (name == ".cctor") continue;
                    IEnumerable<MethodBase> methods = name == ".ctor"
                        ? type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Cast<MethodBase>()
                        : type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).Where(m => m.Name == name).Cast<MethodBase>();
                    foreach (MethodBase method in methods)
                    {
                        if (!patched.Add(method) || method.GetMethodBody() == null) continue;
                        try { harmony.Patch(method, transpiler: new HarmonyMethod(typeof(DisplayTranslation), "Transpiler")); }
                        catch (Exception e) { Warn("Display method skipped: " + target + ": " + e.GetType().Name); }
                    }
                }
                foreach (string target in new[] {
                    "Moody.Table.Column::get_Label", "Moody.Table.Column::get_Tooltip",
                    "Moody.MoodyTab::get_Label", "Moody.Models.TemperatureBandInfo::get_Tooltip" })
                {
                    int split = target.LastIndexOf("::", StringComparison.Ordinal);
                    Type type = assembly.GetType(target.Substring(0, split), false);
                    MethodInfo method = type == null ? null : AccessTools.Method(type, target.Substring(split + 2));
                    if (method != null) harmony.Patch(method, postfix: new HarmonyMethod(typeof(DisplayTranslation), "DisplayResult"));
                }
            }
            catch (Exception e) { Warn("Localization initialization failed: " + e); }
        }
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase __originalMethod)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);
            string target = __originalMethod.DeclaringType.FullName + "::" + __originalMethod.Name;
            code = LocalizeDefinitionDisplay(code, target);
            List<Rule> rules;
            if (!Rules.TryGetValue(target, out rules)) return code;
            foreach (Rule rule in rules)
            {
                int count = code.Count(i => i.opcode == OpCodes.Ldstr && (i.operand as string) == rule.Text);
                if (count != rule.Expected)
                {
                    Warn("Display text layout changed; original method retained: " + target);
                    return code;
                }
            }
            Dictionary<string, int> occurrence = new Dictionary<string, int>(StringComparer.Ordinal);
            List<CodeInstruction> output = new List<CodeInstruction>();
            MethodInfo localize = AccessTools.Method(typeof(DisplayTranslation), "Localize");
            foreach (CodeInstruction instruction in code)
            {
                output.Add(instruction);
                if (instruction.opcode != OpCodes.Ldstr) continue;
                string original = instruction.operand as string;
                Rule rule = rules.FirstOrDefault(r => r.Text == original);
                if (rule == null) continue;
                int index;
                if (!occurrence.TryGetValue(original, out index)) index = 0;
                occurrence[original] = index + 1;
                if (Array.IndexOf(rule.Occurrences, index) >= 0)
                    output.Add(new CodeInstruction(OpCodes.Call, localize));
            }
            return output;
        }
        public static void DisplayResult(ref string __result) { __result = Localize(__result); }
        // Only these three display sites read a defName as a caption. Settings and
        // grouping continue to use the original Def objects and identifiers.
        static List<CodeInstruction> LocalizeDefinitionDisplay(List<CodeInstruction> code, string target)
        {
            bool group = target == "Moody.Views.ThoughtsView::DrawThoughtGroupTable"
                || target == "Moody.Table.Columns.ThoughtGroupColumn::CellRenderer";
            bool skill = target == "Moody.Table.Columns.SkillSummaryColumn::.ctor";
            if (!group && !skill) return code;
            List<int> matches = new List<int>();
            for (int i = 0; i < code.Count; i++)
            {
                FieldInfo field = code[i].operand as FieldInfo;
                if (code[i].opcode != OpCodes.Ldfld || field == null || field.Name != "defName"
                    || field.DeclaringType.FullName != "Verse.Def") continue;
                if (group && (i + 1 >= code.Count || !(code[i + 1].operand is MethodInfo)
                    || ((MethodInfo)code[i + 1].operand).Name != "FormatCamelCaseToSpaces")) continue;
                matches.Add(i);
            }
            if (matches.Count != 1) { Warn("Definition display layout changed; retained: " + target); return code; }
            int at = matches[0];
            code[at] = new CodeInstruction(code[at]);
            code[at].opcode = OpCodes.Call;
            code[at].operand = AccessTools.Method(typeof(DisplayTranslation), group ? "ThoughtGroupName" : "SkillName");
            if (group)
            {
                code[at + 1] = new CodeInstruction(code[at + 1]);
                code[at + 1].opcode = OpCodes.Nop;
                code[at + 1].operand = null;
            }
            return code;
        }
        static object ReadField(object value, string name)
        { return value == null ? null : AccessTools.Field(value.GetType(), name).GetValue(value); }
        static bool IsJapanese()
        {
            Type database = AccessTools.TypeByName("Verse.LanguageDatabase");
            object language = AccessTools.Field(database, "activeLanguage").GetValue(null);
            string folder = ReadField(language, "folderName") as string;
            return folder != null && folder.StartsWith("Japanese", StringComparison.OrdinalIgnoreCase);
        }
        public static string SkillName(object def)
        {
            string original = ReadField(def, "defName") as string;
            try
            {
                if (IsJapanese())
                {
                    string label = ReadField(def, "label") as string;
                    if (!string.IsNullOrEmpty(label)) return label;
                }
            }
            catch { }
            return original;
        }
        public static string ThoughtGroupName(object def)
        {
            string original = ReadField(def, "defName") as string;
            try
            {
                if (IsJapanese())
                {
                    string label = ReadField(def, "label") as string;
                    if (!string.IsNullOrEmpty(label)) return label;
                    // A group can contain several stages. Use its first named
                    // stage as a representative caption, without changing its key.
                    var stages = ReadField(def, "stages") as System.Collections.IEnumerable;
                    if (stages != null) foreach (object stage in stages)
                    {
                        label = ReadField(stage, "label") as string;
                        if (!string.IsNullOrEmpty(label)) return label;
                    }
                }
                MethodInfo format = AccessTools.Method(AccessTools.TypeByName("Moody.Extensions.String"), "FormatCamelCaseToSpaces");
                if (format != null) return (string)format.Invoke(null, new object[] { original });
            }
            catch { }
            return original;
        }
        public static string Localize(string original)
        {
            string key;
            if (original == null || !Texts.TryGetValue(original, out key)) return original;
            try
            {
                Type database = AccessTools.TypeByName("Verse.LanguageDatabase");
                object language = AccessTools.Field(database, "activeLanguage").GetValue(null);
                if (language == null) return original;
                string folder = AccessTools.Field(language.GetType(), "folderName").GetValue(language) as string;
                if (folder == null || !folder.StartsWith("Japanese", StringComparison.OrdinalIgnoreCase)) return original;
                if (translate == null) translate = AccessTools.Method(AccessTools.TypeByName("Verse.Translator"), "Translate", new[] { typeof(string) });
                object translated = translate.Invoke(null, new object[] { key });
                return translated == null ? original : translated.ToString();
            }
            catch { return original; }
        }
        static void Warn(string message)
        {
            Type log = AccessTools.TypeByName("Verse.Log");
            if (log != null) AccessTools.Method(log, "Warning", new[] { typeof(string) }).Invoke(null, new object[] { "[Moody Japanese] " + message });
        }
    }
}
