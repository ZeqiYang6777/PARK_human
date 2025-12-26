using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class StringSafetyChecker : EditorWindow
{
    private Vector2 scrollPosition;
    private List<IssueReport> issues = new List<IssueReport>();
    private bool isScanning = false;

    private class IssueReport
    {
        public string filePath;
        public int lineNumber;
        public string lineContent;
        public string issueType;
        public string severity;
    }

    [MenuItem("Tools/String Safety Checker")]
    public static void ShowWindow()
    {
        GetWindow<StringSafetyChecker>("String Safety Checker");
    }

    void OnGUI()
    {
        GUILayout.Label("C# String Safety Checker", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Scan Project", GUILayout.Height(30)))
        {
            ScanProject();
        }

        GUILayout.Space(10);

        if (issues.Count > 0)
        {
            GUILayout.Label("Found " + issues.Count + " issues:", EditorStyles.boldLabel);

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            foreach (var issue in issues)
            {
                DrawIssue(issue);
            }

            GUILayout.EndScrollView();
        }
        else if (!isScanning)
        {
            GUILayout.Label("No issues found or scan not started.");
        }
    }

    void DrawIssue(IssueReport issue)
    {
        EditorGUILayout.BeginVertical("box");

        Color color = Color.white;
        if (issue.severity == "High") color = Color.red;
        else if (issue.severity == "Medium") color = Color.yellow;

        GUI.color = color;
        GUILayout.Label(issue.issueType, EditorStyles.boldLabel);
        GUI.color = Color.white;

        GUILayout.Label("File: " + issue.filePath);
        GUILayout.Label("Line: " + issue.lineNumber);
        GUILayout.Label("Code: " + issue.lineContent);

        if (GUILayout.Button("Open File"))
        {
            string fullPath = Path.Combine(Application.dataPath, issue.filePath);
            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(fullPath, issue.lineNumber);
        }

        EditorGUILayout.EndVertical();
        GUILayout.Space(5);
    }

    void ScanProject()
    {
        isScanning = true;
        issues.Clear();

        string[] csFiles = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);

        foreach (string file in csFiles)
        {
            ScanFile(file);
        }

        isScanning = false;

        Debug.Log("Scan complete. Found " + issues.Count + " issues.");
    }

    void ScanFile(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            int lineNumber = i + 1;

            // Check for string interpolation
            if (Regex.IsMatch(line, @"\$""[^""]*\{[^}]+\}"))
            {
                AddIssue(filePath, lineNumber, line, "String Interpolation", "High");
            }

            // Check for string.Format
            if (line.Contains("string.Format"))
            {
                AddIssue(filePath, lineNumber, line, "string.Format", "High");
            }

            // Check for emojis
            if (Regex.IsMatch(line, @"[\u2600-\u27BF\U0001F300-\U0001F6FF]"))
            {
                AddIssue(filePath, lineNumber, line, "Emoji Character", "High");
            }
        }
    }

    void AddIssue(string filePath, int lineNumber, string lineContent, string issueType, string severity)
    {
        issues.Add(new IssueReport
        {
            filePath = filePath.Replace(Application.dataPath, "Assets"),
            lineNumber = lineNumber,
            lineContent = lineContent.Trim(),
            issueType = issueType,
            severity = severity
        });
    }
}
