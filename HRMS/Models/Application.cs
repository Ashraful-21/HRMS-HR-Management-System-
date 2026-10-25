using System;
using System.Collections.Generic;

namespace HRMS.Models;

public partial class Application
{
    public int Id { get; set; }

    public int JobOpeningId { get; set; }

    public int CandidateId { get; set; }

    public string Stage { get; set; } = null!;

    public int StageOrder { get; set; }

    public string? Notes { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual Candidate Candidate { get; set; } = null!;

    public virtual ICollection<Interview> Interviews { get; set; } = new List<Interview>();

    public virtual JobOpening JobOpening { get; set; } = null!;

    public virtual ICollection<OfferLetter> OfferLetters { get; set; } = new List<OfferLetter>();
}
