1.I string array surašomi keliai iki kiekvieno failo pasirinktame aplanke.
2.Iš to string array generuojama Queue<string> i kure surašoma informacija taip kaip bus palyginami failai iš eiles.
3.Gijų sinchronizavimas.
Visu pirma yra tikrinama gijų sinchronizavima užtikrina Lock ir Monitor.Enter.
Tai yra naudojama dvejose vietose:
1-Tada kai gija gauna užduoti.
2-Tada kai gija Irašo rezultata i faila.
Kiekviena iš šiu atveju užtikrina atskiras raktas:object key_saving; ir object key_comparing;
Kai gija Ivykdo užduoti(patikrina ar du failai yra dublikatais) ji bando gauti nauja užduoti(tam kad kiekviena gija gautu savo užduoti ir naudojama Queue ir Monitor.Enter);
4.Kiekviena gija lygina tarpusavyje 2 failus. Jai tie failai yra dublikatais tai ji ir Irašo ju pavadinimus i out faila.
5.Kad pagreitinti programos veikima:
1.Visu pirma yra tikrinama ant sutapimo pirma Char dveju failu.Tokiu budu atmintis nera perkraunama nes nuskaitomas nuo failo tik vienas simbolis.
2.Jai char sutapo tada nuskaitoma is failu po eilute ir jos yra tikrinamos. Nuskaitome eilute nes tikrini eilute su eilute užima mažiau laiko nei tikrinti visa faila char by char. Už tai reiks moketi tuo kad eilute užima daugiau vietos atmintyje.
3.Kai gija randa nesutapima toliau netikrina ir gauna nauja uzduoti.(Taigi failai nuskaitomi iki galo tik tada jai jie yra dublikatais).

