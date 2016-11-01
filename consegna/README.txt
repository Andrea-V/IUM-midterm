- Piattaforma:
Microsoft Visual Studio Enterprise 2015
F# for Visual Studio v2.0.0.0


- ESERCIZIO 1:
La soluzione dell'esercizio 1 è in questa cartella, sotto nome "es1.html".

-ESERCIZI 2 & 3:
Le soluzioni dell'esercizio 2 e 3 (+ alcuni file ausiliari) sono nella sottocartella "./midterm/".

I nomi dei files di soluzione del secondo e del terzo esercizio sono, rispettivamente, "es2.fsx" ed "es3.fsx". 

In testa ai file sorgenti è brevemente spiegato come utilizzare i rispettivi programmi.


- DISCUSSIONE DELLA SOLUZIONE DELL'ESERCIZIO 2:

L'agoritmo scelto è il 4-Way Flood Fill.

Dato un punto d'inizio "p", un colore iniziale "c_i" ed un colore finale "c_f", algoritmo:
- Colora p di colore c_f.
- Si richiama ricorsivamente sui punti limitrofi di colore c_i (escludendo le diagonali).

Il 4-way flood fill è in grado di eseguire il fill di tutti i poligoni semplici, sia concavi che convessi. Rispetto al 8-way flood fill, che si richiama anche sui punti limitrofi diagonali, il 4-way ff funziona egregiamente anche in assenza di anti-aliasing; l'8-way ff, d'altro canto, potrebbe "scavalcare" il perimetro del poligono, andando quindi a riempira tutta l'area di vista.
A differenza degli algoritmi di tipo scan-line, non è necessario conoscere le coordinate dei vertici del poligono per eseguire il 4-way ff, tuttavia occorre che l'area all'interno del poligono sia gia' colorata in modo uniforme.

Dato l'elevato numero di chiamate ricorsive (1 per ogni pixel contenuto nell'area del poligono), un'implementazione ricorsiva dell'algoritmo puo' causare uno stack overflow anche per immagini di dimensioni modeste. Per questo motivo, nell'esercizio 2 si utilizza un'implementazione
iterativa, che utilizza uno stack esplicito.