# NodeCanvas Task Creation Skill

## Description
**WORKFLOW SKILL** — Create, customize, and implement NodeCanvas action and condition tasks for Unity projects. USE FOR: generating new task classes for integrations like EasySave, PlayableDirector, or custom systems; ensuring tasks follow NodeCanvas best practices; adding proper attributes, parameters, and error handling. DO NOT USE FOR: creating composite tasks, decorators, or complex graph structures; runtime debugging of existing tasks; non-NodeCanvas scripting.

## Workflow

### Step 1: Determine Task Type
- **ConditionTask**: For checking boolean conditions (e.g., file exists, key exists).
- **ActionTask**: For performing operations (e.g., save data, load data, delete file).

### Step 2: Set Up Class Structure
- Inherit from `ConditionTask` or `ActionTask<T>` where T is the agent type (use `ActionTask` if no specific agent).
- Use generics for type flexibility (e.g., `ActionTask<T>` for saving/loading any type).
- Place in appropriate namespace: `NodeCanvas.Tasks.[Category]` (e.g., `NodeCanvas.Tasks.EasySave`).

### Step 3: Add Attributes
```csharp
[Category("Extensions/[Integration]/Utility")]  // Organize in NodeCanvas menu
[Name("Task Display Name")]                     // Friendly name in editor
[Description("Brief description of what the task does")]
```

### Step 4: Define Parameters
- Use `BBParameter<T>` for blackboard-compatible parameters.
- Add `[BlackboardOnly]` for output parameters.
- Provide default values where appropriate.

### Step 5: Implement Core Logic
- **For ConditionTask**: Override `protected override bool OnCheck()` and return the condition result.
- **For ActionTask**: Override `protected override void OnExecute()` and call `EndAction()` or `EndAction(bool)` when done.
- Handle errors with `try-catch` and `Error(string)` method.
- Use coroutines if needed for async operations.

### Step 6: Add Optional Info Property
```csharp
protected override string info
{
    get { return string.Format("Action: {0}", key.value); }
}
```

### Step 7: Test and Verify
- Compile in Unity and check for errors.
- Add to a NodeCanvas graph and verify parameters appear correctly.
- Test execution in play mode.

## Code Templates

### ConditionTask Template
```csharp
using UnityEngine;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace NodeCanvas.Tasks.[Category]
{
    [Category("Extensions/[Integration]/Utility")]
    [Name("Check [Condition]")]
    [Description("Checks if [condition description]")]
    public class Check[Condition] : ConditionTask
    {
        public BBParameter<string> parameter;

        protected override bool OnCheck()
        {
            // Implement condition check
            return [boolean result];
        }
    }
}
```

### ActionTask Template
```csharp
using UnityEngine;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace NodeCanvas.Tasks.[Category]
{
    [Category("Extensions/[Integration]/Utility")]
    [Name("Do [Action]")]
    [Description("Performs [action description]")]
    public class Do[Action] : ActionTask
    {
        public BBParameter<string> inputParam;
        [BlackboardOnly]
        public BBParameter<string> outputParam;

        protected override void OnExecute()
        {
            try
            {
                // Implement action logic
                outputParam.value = [result];
                EndAction();
            }
            catch (System.Exception e)
            {
                Error("Failed to [action]: " + e.Message);
                EndAction(false);
            }
        }
    }
}
```

### Generic ActionTask Template
```csharp
using UnityEngine;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace NodeCanvas.Tasks.[Category]
{
    [Category("Extensions/[Integration]/Saving")]
    [Name("Save [Type]")]
    [Description("Saves [type] data to [system]")]
    public class Save[Type]<T> : ActionTask
    {
        public BBParameter<string> key;
        public BBParameter<T> data;
        public BBParameter<string> filePath = "SaveFile.[ext]";

        protected override void OnExecute()
        {
            try
            {
                // Save logic here
                ES3.Save(key.value, data.value, filePath.value);
                EndAction();
            }
            catch (System.Exception e)
            {
                Error("Save failed: " + e.Message);
                EndAction(false);
            }
        }
    }
}
```

## Examples from EasySaveTasks.cs

### ConditionTask Example: ES3FileExists
```csharp
[Category("Extensions/EasySave/Utility")]
public class ES3FileExists : ConditionTask
{
    public BBParameter<string> path = "SaveFile.es3";
    protected override bool OnCheck() {
        return ES3.FileExists(path.value);
    }
}
```

### ActionTask Example: ES3Save<T>
```csharp
[Category("Extensions/EasySave/Saving")]
public class ES3Save<T> : ActionTask
{
    public BBParameter<string> key;
    public BBParameter<T> data;
    public BBParameter<string> filePath = "SaveFile.es3";
    public ES3.EncryptionType encryptionType = ES3.EncryptionType.None;
    public string encryptionPassword = "password";
    public ES3.CompressionType compressionType = ES3.CompressionType.None;

    protected override void OnExecute() {
        var settings = new ES3Settings(filePath.value, encryptionType, encryptionPassword);
        settings.compressionType = compressionType;
        try {
            ES3.Save(key.value, data.value, settings);
            EndAction();
        }
        catch (System.FormatException) {
            ES3.DeleteFile(settings);
            try {
                ES3.Save(key.value, data.value, settings);
                EndAction();
            }
            catch (System.Exception e) {
                Error("ES3.Save failed after deleting corrupt file: " + e.Message);
                EndAction(false);
            }
        }
        catch (System.Exception e) {
            Error("ES3.Save failed: " + e.Message);
            EndAction(false);
        }
    }
}
```

## Best Practices
- Always call `EndAction()` in ActionTask to prevent hanging.
- Use `[BlackboardOnly]` for parameters that should only be set by the task.
- Handle exceptions gracefully and provide meaningful error messages.
- Test with different data types and edge cases.
- Follow the project's namespace conventions (e.g., `NodeCanvas.Tasks.[Integration]`).

## Dependencies
- NodeCanvas Framework
- ParadoxNotion.Design
- UnityEngine
- Integration-specific libraries (e.g., ES3 for EasySave)