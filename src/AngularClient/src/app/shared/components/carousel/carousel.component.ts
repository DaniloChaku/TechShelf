import { Component, Input, OnInit } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-carousel',
  standalone: true,
  imports: [MatIconModule],
  templateUrl: './carousel.component.html',
  styleUrl: './carousel.component.css',
})
export class CarouselComponent {
  @Input() items: string[] = [];
  currentSlide = 0;

  prev() {
    this.currentSlide =
      this.currentSlide === 0
        ? this.items.length - 1
        : this.currentSlide - 1;
  }

  next() {
    this.currentSlide =
      this.currentSlide === this.items.length - 1
        ? 0
        : this.currentSlide + 1;
  }

  goToSlide(index: number) {
    this.currentSlide = index;
  }
}
