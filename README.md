# Sudoku Solver
Program na vyřešení sudoku
![fotka-programu]

## Funkce:
- Vyřešit sudoku
- Vygenerovat sudoku
- Uložení sudoku
- Načtení sudoku
- Řešení sudoku
- Automatické přizpůsobení pro různé monitory

### Vyřešit sudoku
- Lze sledovat, jak jsou políčka průběžně vyplňována
	- V případě zaškrtnutí možnosti se program při hledání řešení zastaví na specifikované množství času po každém pokusu
- Upozornění v případě 
	- neplatného zadání
	- neexistujícího řešení

![prubezne-sledovani]
### Vytváření sudoku
- Zabudován algoritmus na vytvoření sudoku, které má vždy jen a pouze jedno řešení
- Několik možných úrovní obtížnosti
	- Lehká obtížnost
	- Střední obtížnost
	- Těžká obtížnost
	
![generovani-sudoku]

### Načítání & ukládání
- Program ukládá sudoku do mřížky formátu [Simple Sudoku][simple-sudoku] (*.ss), která je pro člověka lehce pochopitelná

<div align="center">
    <img src="https://i.imgur.com/OOpF75b.png" width=205 height=338>
</div>

### Manuální řešení sudoku
- Uživatel si může vygenerovat sudoku různé obtížnosti a následně jej sám řešit
- Při vyřešení je uživatel informován
- V průběhu řešení má uživatel možnost zkontrolovat si, jestli neudělal v řešení chybu

![manualni-reseni]
### Automatické přizpůsobení pro různé monitory
###### sice ne vždy ideálně, ale to nevadí
- Měřítko programu se přizpůsobí tak, aby program byl použitelný na různých zařízeních

[simple-sudoku]: https://www.sudocue.net/fileformats.php
[fotka-programu]: https://i.imgur.com/es00QYu.png
[prubezne-sledovani]: aaaaaaaa
[manualni-reseni]: https://i.imgur.com/trh7Vki.png
[generovani-sudoku]: https://i.imgur.com/f3Xrci8.png
