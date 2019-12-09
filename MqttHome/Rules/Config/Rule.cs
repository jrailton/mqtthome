using log4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MqttHome
{
    public class Rule
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Switch { get; set; }

        public DateTime? StateChanged { get; set; }
        public bool? State { get; set; }

        /// <summary>
        /// Any of these conditions can be matched to return true
        /// </summary>
        public List<string> ConditionsOr { get; set; } = new List<string>();

        /// <summary>
        /// All of these conditions must be matched to return true
        /// </summary>
        public List<string> ConditionsAnd { get; set; } = new List<string>();

        public int? FlipFlop { get; set; }

        public bool DependsOnCondition(string condition) {
            return (ConditionsOr.Any(c => c == condition) || ConditionsAnd.Any(c => c == condition));
        }

        /// <summary>
        /// Will return true if conditions are matched. Errs on the side of caution, if a condition value is null its presumed to be FALSE
        /// </summary>
        public bool Test(List<Condition> c, ILog logger) {
            bool result;

            var conditions = c.ToDictionary(o => o.Id, o => o.DeviceCondition);

            string logIdentity = $"Test :: Rule: {Name}, Switch: {Switch}";

            logger.Debug($@"{logIdentity}
{string.Join(Environment.NewLine, c.Select(s => $"Condition {s.Id}: {s.DeviceCondition}"))}");

            foreach (var conditionId in ConditionsAnd)
            {
                var temp = conditions[conditionId];

                logger.Debug($"{logIdentity} :: AND :: {conditionId} = {temp}{(temp.HasValue ? "" : " (will default to FALSE and exit early)")}");

                result = temp ?? false;

                // exit early if false
                if (!result)
                {
                    logger.Debug($"{logIdentity} :: AND :: Returning early, result FALSE");
                    return false;
                }
            }

            foreach (var conditionId in ConditionsOr) {
                var temp = conditions[conditionId];

                logger.Debug($"{logIdentity} :: OR :: {conditionId} = {temp}{(temp.HasValue ? "" : " (will default to FALSE and continue checking)")}");

                result = temp ?? false;

                // exit early if true
                if (result)
                {
                    logger.Debug($"{logIdentity} :: AND :: Returning early, result TRUE");
                    return true;
                }
            }

            logger.Debug($"{logIdentity} :: BOTH :: Returning result FALSE");
            return false;
        }
    }
}
