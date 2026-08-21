using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExitInterviewSystem.Models
{
    public class ExitInterviewForm
    {
        public int Id { get; set; }

        // 1. Personal Details
        [StringLength(150)]
        public string? Name { get; set; }

        [StringLength(50)]
        [Display(Name = "Persal No")]
        public string? PersalNo { get; set; }

        [StringLength(50)]
        [Display(Name = "ID No")]
        public string? IDNo { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(100)]
        [Display(Name = "Post / Salary Level")]
        public string? PostSalaryLevel { get; set; }

        [StringLength(100)]
        public string? Rank { get; set; }

        [StringLength(50)]
        public string? Race { get; set; }

        [StringLength(20)]
        public string? Gender { get; set; }

        [Display(Name = "Date of Appointment")]
        [DataType(DataType.Date)]
        public DateTime? DateOfAppointment { get; set; }

        [Display(Name = "Date of Entry to Current Position")]
        [DataType(DataType.Date)]
        public DateTime? DateOfEntryToCurrentPosition { get; set; }

        [StringLength(200)]
        [Display(Name = "Institution / Office / Component")]
        public string? InstitutionOfficeComponent { get; set; }

        public int? InstitutionId { get; set; }

        [ForeignKey(nameof(InstitutionId))]
        public Institution? Institution { get; set; }

        [Display(Name = "Date of Termination")]
        [DataType(DataType.Date)]
        public DateTime? DateOfTermination { get; set; }

        public int? FinancialYearId { get; set; }

        [ForeignKey(nameof(FinancialYearId))]
        public FinancialYear? FinancialYear { get; set; }

        // 2. Exit Information
        [StringLength(100)]
        [Display(Name = "Termination Type")]
        public string? TerminationType { get; set; }

        [StringLength(200)]
        [Display(Name = "Other (specify)")]
        public string? TerminationTypeOtherText { get; set; }

        [Display(Name = "Main Reason for Leaving")]
        public string? MainReasonForLeaving { get; set; }

        [StringLength(10)]
        [Display(Name = "Treated Fairly?")]
        public string? TreatedFairly { get; set; }

        public string? TreatedFairlyReason { get; set; }

        [StringLength(10)]
        [Display(Name = "Would Consider Returning?")]
        public string? WouldConsiderReturning { get; set; }

        public string? WouldConsiderReturningReason { get; set; }

        [StringLength(10)]
        [Display(Name = "Paid Adequate Salary?")]
        public string? PaidAdequateSalary { get; set; }

        public string? PaidAdequateSalaryReason { get; set; }

        [Display(Name = "Conditions to Have Stayed")]
        public string? ConditionsToHaveStayed { get; set; }

        [Display(Name = "What Would You Change")]
        public string? WhatWouldYouChange { get; set; }

        [StringLength(10)]
        [Display(Name = "Contributions Recognised?")]
        public string? ContributionsRecognised { get; set; }

        public string? ContributionsRecognisedReason { get; set; }

        [StringLength(10)]
        [Display(Name = "Understood Policies?")]
        public string? UnderstoodPolicies { get; set; }

        public string? UnderstoodPoliciesReason { get; set; }

        [Display(Name = "Illegal Acts Witnessed")]
        public string? IllegalActsWitnessed { get; set; }

        [StringLength(10)]
        [Display(Name = "Training Adequate?")]
        public string? TrainingAdequate { get; set; }

        public string? TrainingAdequateReason { get; set; }

        [StringLength(10)]
        [Display(Name = "Satisfied with Working Conditions?")]
        public string? SatisfiedWorkingConditions { get; set; }

        public string? SatisfiedWorkingConditionsReason { get; set; }

        [Display(Name = "Security Arrangements Comment")]
        public string? SecurityArrangementsComment { get; set; }

        [Display(Name = "Morale Suggestions")]
        public string? MoraleSuggestions { get; set; }

        [Display(Name = "Liked Most About Position")]
        public string? LikedMostAboutPosition { get; set; }

        [Display(Name = "Liked Least About Position")]
        public string? LikedLeastAboutPosition { get; set; }

        [StringLength(10)]
        [Display(Name = "Could Have Prevented Leaving?")]
        public string? CouldHavePreventedLeaving { get; set; }

        public string? CouldHavePreventedLeavingReason { get; set; }

        [StringLength(10)]
        [Display(Name = "Goals and Targets Clear?")]
        public string? GoalsAndTargetsClear { get; set; }

        public string? GoalsAndTargetsClearReason { get; set; }

        [StringLength(10)]
        [Display(Name = "Qualification/Skills Better Used?")]
        public string? QualificationSkillsBetterUsed { get; set; }

        public string? QualificationSkillsBetterUsedReason { get; set; }

        [Display(Name = "General Comments")]
        public string? GeneralComments { get; set; }

        [StringLength(150)]
        [Display(Name = "Signature of Employee")]
        public string? SignatureOfEmployee { get; set; }

        [Display(Name = "Signature Date")]
        [DataType(DataType.Date)]
        public DateTime? SignatureDate { get; set; }

        [StringLength(100)]
        [Display(Name = "Captured By")]
        public string? CapturedBy { get; set; }

        public DateTime DateCaptured { get; set; } = DateTime.Now;
    }
}
