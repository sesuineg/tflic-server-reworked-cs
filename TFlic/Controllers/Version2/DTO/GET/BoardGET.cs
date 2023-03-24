﻿using TFlic.Models.Domain.Organization.Project;

namespace TFlic.Controllers.Version2.DTO.GET;

public class BoardGet
{
    public BoardGet(Board board)
    {
        Id = board.Id;
        Name = board.Name;
        if (board.Columns == null) return;
        foreach (var column in board.Columns) { Columns.Add(column.Id); }
    }
    
    public ulong Id { get; set; }
    public string Name { get; set; }
    public List<ulong> Columns { get; } = new();
}