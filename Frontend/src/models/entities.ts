export interface Entry {
    id?: string;
    text: string;
    intervals: number[];
    time: Date;
    printDate: boolean;
    string?: string;
}