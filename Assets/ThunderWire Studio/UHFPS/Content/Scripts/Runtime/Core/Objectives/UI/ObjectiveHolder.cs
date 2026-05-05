using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using UnityEngine;
using UHFPS.Tools;
using TMPro;
using ThunderWire.Attributes;
using static UHFPS.Runtime.ObjectiveManager;

namespace UHFPS.Runtime
{
    [InspectorHeader("Objective Holder")]
    public class ObjectiveHolder : MonoBehaviour
    {
        public Transform SubObjectives;
        public TMP_Text ObjectiveTitle;

        private ObjectiveManager manager;
        private readonly CompositeDisposable disposables = new();
        private readonly Dictionary<string, GameObject> subObjectives = new();
        private readonly Dictionary<string, CompositeDisposable> subDisposables = new();

        private void OnDestroy()
        {
            disposables.Dispose();
            foreach (var disposable in subDisposables)
            {
                disposable.Value.Dispose();
            }
        }

        public void SetObjective(ObjectiveManager manager, ObjectiveData objective)
        {
            this.manager = manager;
            ObjectiveTitle.text = objective.Objective.ObjectiveTitle;

            // subscribe listening to localization changes
            objective.Objective.ObjectiveTitle
                .ObserveText(text => ObjectiveTitle.text = text)
                .AddTo(disposables);

            // event when all objectives will be completed
            objective.IsCompleted.Subscribe(completed =>
            {
                if (completed)
                {
                    disposables.Dispose();
                    Destroy(gameObject);
                }
            })
            .AddTo(disposables);

            // event when sub objective will be added
            objective.AddSubObjective.Subscribe(data =>
            {
                CreateSubObjective(data);
            })
            .AddTo(disposables);

            // event when sub objective will be removed
            objective.RemoveSubObjective.Subscribe(RemoveSubObjective)
            .AddTo(disposables);

            // add starting objectives
            foreach (var subObj in objective.SubObjectives)
            {
                CreateSubObjective(subObj.Value);
            }
        }

        private void CreateSubObjective(SubObjectiveData data)
        {
            GameObject subObjective = Instantiate(manager.SubObjectivePrefab, Vector3.zero, Quaternion.identity, SubObjectives);
            TMP_Text objectiveTitle = subObjective.GetComponentInChildren<TMP_Text>();
            data.SubObjectiveObject = subObjective;

            CompositeDisposable _disposables = new();
            subDisposables.Add(data.SubObjective.SubObjectiveKey, _disposables);

            string subObjectiveText = data.SubObjective.ObjectiveText;
            objectiveTitle.text = FormatObjectiveText(subObjectiveText, data.CompleteCount.Value);

            // subscribe listening to localization changes
            data.SubObjective.ObjectiveText
                .ObserveText(text => objectiveTitle.text = FormatObjectiveText(text, data.CompleteCount.Value))
                .AddTo(_disposables);

            // event when sub objective will be completed
            data.IsCompleted.Subscribe(completed =>
            {
                if (completed)
                {
                    _disposables.Dispose();
                    Destroy(subObjective);
                }
            })
            .AddTo(_disposables);

            // event when sub objective complete count will be changed
            data.CompleteCount.Subscribe(count =>
            {
                objectiveTitle.text = FormatObjectiveText(subObjectiveText, count);
            })
            .AddTo(_disposables);

            // add sub objective to sub objectives dictionary
            subObjectives.Add(data.SubObjective.SubObjectiveKey, subObjective);
        }

        private void RemoveSubObjective(string key)
        {
            if(subObjectives.TryGetValue(key, out GameObject subObj))
            {
                Destroy(subObj);
                subDisposables[key].Dispose();
                subDisposables.Remove(key);
            }
        }

        private string FormatObjectiveText(string text, ushort count)
        {
            return text.RegexReplaceTag('[', ']', "count", count.ToString());
        }
    }
}