module Engine.Domain

open System

module InputData =
    type Fund = {
        Id : Guid
        GrowthRate : decimal
        Charge : decimal
    }

open InputData

type Inputs = {
    Investment : decimal
    InvestmentPercentages : (Guid * decimal) seq
    Funds : Fund seq
}

[<Struct>]
type Dictionary<'TKey, 'TValue when 'TKey : equality> (data : ('TKey * 'TValue) seq) =
    member private this.Dict =
        let dict = System.Collections.Generic.Dictionary ()
        data |> Seq.iter (fun (key, value) -> dict[key] <- value)
        dict

    member this.Item
        with get key =
            this.Dict[key]
