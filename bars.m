close all
clc
clear all

figure
barh(1, [6 31 25], 'stacked')
hold on
set(gca,'YTick',1,'YTickLabel',"             Range Divisions")
xlim([0 62])
ylim([0 3.5])
legend('Max 15^\circ', 'Max 45^\circ', 'Max 75^\circ')
pbaspect([6 1 1])
text(3, 1, '6 (9.7%)', 'HorizontalAlignment', 'center')
text(6 + 15.5, 1, '31 (50.0%)', 'HorizontalAlignment', 'center')
text(6 + 31 + 12.5, 1, '25 (40.3%)', 'HorizontalAlignment', 'center')
set(gca().YAxis, 'TickLength', [0 0])
ytickangle(90)
hold off

figure
barh(1, [0.475 0.25 0.275], 'stacked')
hold on
set(gca,'YTick',1,'YTickLabel',"             Range Divisions")
xlim([0 1])
ylim([0 3.5])
legend('Turn towards threat', 'Turn from threat', 'No action')
pbaspect([6 1 1])
text(0.475/2, 1, '0.475', 'HorizontalAlignment', 'center')
text(0.475+0.25/2, 1, '0.250', 'HorizontalAlignment', 'center')
text(1-0.275/2, 1, '0.275', 'HorizontalAlignment', 'center')
set(gca().YAxis, 'TickLength', [0 0])
ytickangle(90)
hold off

figure
barh(1, [0.741 1-0.741], 'stacked')
hold on
set(gca,'YTick',1,'YTickLabel',"             Range Divisions")
xlim([0 1])
ylim([0 3.5])
legend('Avoid threat', 'No action')
pbaspect([6 1 1])
text(0.741/2, 1, '0.741', 'HorizontalAlignment', 'center')
text(1-0.259/2, 1, '0.259', 'HorizontalAlignment', 'center')
set(gca().YAxis, 'TickLength', [0 0])
ytickangle(90)
hold off
