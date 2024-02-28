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
    <img src="https://media.discordapp.net/attachments/1076565079333548184/1128794184904613908/2023-07-12_23_04_41-C__Users_orbit_Documents_sodoku.ss_-_Sublime_Text_UNREGISTERED.png?width=413&height=676" width=205 height=338>
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
[fotka-programu]: https://i.imgur.com/qPg8gFS.png
[prubezne-sledovani]: https://cdn.discordapp.com/attachments/1076565079333548184/1129493630118268939/sudoku_solver_timelapse.gif
[manualni-reseni]: https://media.discordapp.net/attachments/1076565079333548184/1129492858345377952/2023-07-14_21_20_48-Sudoku_Solver_Ignac_Brychta_2023.png?width=808&height=676
[generovani-sudoku]: https://media.discordapp.net/attachments/1076565079333548184/1129492858655744092/2023-07-14_21_12_48-Sudoku_Solver_Ignac_Brychta_2023.png?width=808&height=676
