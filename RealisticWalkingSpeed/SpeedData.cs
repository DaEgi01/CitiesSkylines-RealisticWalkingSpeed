using System.Collections.Generic;
using static Citizen;

namespace RealisticWalkingSpeed
{
    public class SpeedData
    {
        private struct AgeRange
        {
            private AgeRange(int ageMin, int ageMax)
            {
                this.AgeMin = ageMin;
                this.AgeMax = ageMax;
            }

            public int AgeMin { get; }
            public int AgeMax { get; }

            public static AgeRange From0to10 => new AgeRange(0, 10);
            public static AgeRange From10to20 => new AgeRange(10, 20);
            public static AgeRange From20to30 => new AgeRange(20, 30);
            public static AgeRange From30to40 => new AgeRange(30, 40);
            public static AgeRange From40to50 => new AgeRange(40, 50);
            public static AgeRange From50to60 => new AgeRange(50, 60);
            public static AgeRange From60to70 => new AgeRange(60, 70);
            public static AgeRange From70to80 => new AgeRange(70, 80);
            public static AgeRange From80to90 => new AgeRange(80, 90);
            public static AgeRange From90to100 => new AgeRange(90, 100);
        }

                private struct AgeRangeAndGender
        {
            public AgeRangeAndGender(AgeRange ageRange, Gender gender)
            {
                this.AgeRange = ageRange;
                this.Gender = gender;
            }

            public AgeRange AgeRange { get; }
            public Gender Gender { get; }
        }

        //sources:
        //http://lermagazine.com/article/self-selected-gait-speed-a-critical-clinical-outcome
        //https://musculoskeletalkey.com/testing-functional-performance/
        private readonly Dictionary<AgeRangeAndGender, float> data = new Dictionary<AgeRangeAndGender, float>()
        {
            { new AgeRangeAndGender(AgeRange.From0to10, Gender.Male), 0.68544f },
            { new AgeRangeAndGender(AgeRange.From0to10, Gender.Female), 0.6624f },
            { new AgeRangeAndGender(AgeRange.From10to20, Gender.Male), 0.70272f },
            { new AgeRangeAndGender(AgeRange.From10to20, Gender.Female), 0.70272f },
            { new AgeRangeAndGender(AgeRange.From20to30, Gender.Male), 0.78336f },
            { new AgeRangeAndGender(AgeRange.From20to30, Gender.Female), 0.77184f },
            { new AgeRangeAndGender(AgeRange.From30to40, Gender.Male), 0.82368f },
            { new AgeRangeAndGender(AgeRange.From30to40, Gender.Female), 0.77184f },
            { new AgeRangeAndGender(AgeRange.From40to50, Gender.Male), 0.82368f },
            { new AgeRangeAndGender(AgeRange.From40to50, Gender.Female), 0.80064f },
            { new AgeRangeAndGender(AgeRange.From50to60, Gender.Male), 0.82368f },
            { new AgeRangeAndGender(AgeRange.From50to60, Gender.Female), 0.75456f },
            { new AgeRangeAndGender(AgeRange.From60to70, Gender.Male), 0.82368f },
            { new AgeRangeAndGender(AgeRange.From60to70, Gender.Female), 0.71424f },
            { new AgeRangeAndGender(AgeRange.From70to80, Gender.Male), 0.72576f },
            { new AgeRangeAndGender(AgeRange.From70to80, Gender.Female), 0.65088f },
            { new AgeRangeAndGender(AgeRange.From80to90, Gender.Male), 0.55872f },
            { new AgeRangeAndGender(AgeRange.From80to90, Gender.Female), 0.54144f },
        };

        private AgeGroup GetAgeGroupFrom(AgePhase agePhase)
        {
            switch (agePhase)
            {
                case AgePhase.Child:
                    return AgeGroup.Child;
                case AgePhase.Teen0:
                case AgePhase.Teen1:
                    return AgeGroup.Teen;
                case AgePhase.Young0:
                case AgePhase.Young1:
                case AgePhase.Young2:
                    return AgeGroup.Young;
                case AgePhase.Adult0:
                case AgePhase.Adult1:
                case AgePhase.Adult2:
                case AgePhase.Adult3:
                    return AgeGroup.Adult;
                case AgePhase.Senior0:
                case AgePhase.Senior1:
                case AgePhase.Senior2:
                case AgePhase.Senior3:
                    return AgeGroup.Senior;
                default:
                    return AgeGroup.Adult;
            }
        }

        private AgeRange GetAgeRangeFrom(AgeGroup ageGroup)
        {
            switch (ageGroup)
            {
                case AgeGroup.Child:
                    return AgeRange.From0to10;
                case AgeGroup.Teen:
                    return AgeRange.From10to20;
                case AgeGroup.Young:
                    return AgeRange.From20to30;
                case AgeGroup.Adult:
                    return AgeRange.From40to50;
                case AgeGroup.Senior:
                    return AgeRange.From80to90;
                default:
                    return AgeRange.From40to50;
            }
        }

        public float GetAverageSpeed(AgePhase agePhase, Gender gender)
        {
            var ageRange = GetAgeRangeFrom(GetAgeGroupFrom(agePhase));
            return data[new AgeRangeAndGender(ageRange, gender)];
        }
    }
}
