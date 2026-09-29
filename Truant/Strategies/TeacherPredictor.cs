using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Truant.Models;

namespace Truant.Strategies
{
    internal class TeacherPredictor
    {
        public PredictionResult Predict(Subject subject, List<DayResult> history)
        {
            var candidates = CreateCandidates();

            double totalWeight = 0;
            double askWeight = 0;

            var weights = new List<double>();

            foreach (RuleCandidate candidate in candidates)
            {
                double weight = CalculateWeight(candidate, subject, history);

                if (weight <= 0)
                {
                    continue;
                }

                totalWeight += weight;
                weights.Add(weight);

                if (candidate.Rule == TeacherRule.Random)
                {
                    askWeight += weight * 0.5;
                }
                else if (PredictForCandidate(candidate, history))
                {
                    askWeight += weight;
                }
            }

            if (totalWeight == 0)
            {
                return new PredictionResult(0.5, 0);
            }

            double askProbability = askWeight / totalWeight;

            double confidence = CalculateConfidence(weights, totalWeight);

            return new PredictionResult(askProbability, confidence);
        }

        private double CalculateConfidence(List<double> weights, double totalWeight)
        {
            if (weights.Count == 0)
            {
                return 0;
            }

            double maxWeight = weights.Max();

            return maxWeight / totalWeight;
        }

        private List<RuleCandidate> CreateCandidates()
        {
            var candidates = new List<RuleCandidate>();

            // Правило 1.
            candidates.Add(new RuleCandidate(TeacherRule.Random));

            var subjects = Enum.GetValues<Subject>();

            // Правило 2.
            foreach (Subject a in subjects)
            {
                candidates.Add(new RuleCandidate(TeacherRule.IfAskedYesterday, a));
            }

            // Правило 3.
            foreach (Subject a in subjects)
            {
                foreach (Subject b in subjects)
                {
                    if (a == b)
                    {
                        continue;
                    }

                    candidates.Add(new RuleCandidate(TeacherRule.IfAskedHistory, a, b));
                }
            }

            return candidates;
        }

        private double CalculateWeight(RuleCandidate candidate, Subject subject, List<DayResult> history)
        {
            if (history.Count == 0)
            {
                return GetPrior(candidate);
            }

            if (candidate.Rule == TeacherRule.Random)
            {
                return GetPrior(candidate) * Math.Pow(0.5, history.Count);
            }

            if (!MatchesHistory(candidate, subject, history))
            {
                return 0;
            }

            return GetPrior(candidate);
        }

        private double GetPrior(RuleCandidate candidate)
        {
            var subjects = Enum.GetValues<Subject>();

            switch (candidate.Rule)
            {
                case TeacherRule.Random:
                    return 1.0 / 3.0;

                case TeacherRule.IfAskedYesterday:
                    return 1.0 / 3.0 / subjects.Length;

                case TeacherRule.IfAskedHistory:
                    return 1.0 / 3.0 / (subjects.Length * (subjects.Length - 1));

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool MatchesHistory(RuleCandidate candidate, Subject subject, List<DayResult> history)
        {
            for (int day = 0; day < history.Count; day++)
            {
                bool actual = history[day].Asked[subject];

                bool expected;

                if (candidate.Rule == TeacherRule.IfAskedYesterday)
                {
                    bool yesterdayAsked;

                    if (day == 0)
                    {
                        yesterdayAsked = false;
                    }
                    else
                    {
                        yesterdayAsked = history[day - 1].Asked[candidate.SubjectA!.Value];
                    }

                    expected = yesterdayAsked;
                }
                else
                {
                    bool askedA;
                    bool askedB;

                    if (day == 0)
                    {
                        askedA = false;
                        askedB = false;
                    }
                    else
                    {
                        askedA = history[day - 1].Asked[candidate.SubjectA!.Value];

                        askedB = history[day - 1].Asked[candidate.SubjectB!.Value];
                    }

                    expected = askedA != askedB;
                }

                if (actual != expected)
                {
                    return false;
                }
            }

            return true;
        }

        private bool


       PredictForCandidate(RuleCandidate candidate, List<DayResult> history)
        {
            if (history.Count == 0)
            {
                return false;
            }

            int lastDay = history.Count - 1;

            switch (candidate.Rule)
            {
                case TeacherRule.Random:
                    return false;

                case TeacherRule.IfAskedYesterday:
                    return history[lastDay].Asked[candidate.SubjectA!.Value];

                case TeacherRule.IfAskedHistory:
                    bool askedA = history[lastDay].Asked[candidate.SubjectA!.Value];

                    bool askedB = history[lastDay].Asked[candidate.SubjectB!.Value];

                    return askedA != askedB;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
